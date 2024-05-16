using Azure;
using Azure.Data.Tables;
using ErrorOr;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Persistence;

internal sealed class InstrumentRepository : IInstrumentRepository
{
	private readonly TableClient tableClient;

	internal InstrumentRepository(TableClient tableClient)
	{
		this.tableClient = tableClient;
	}

	public async Task<ErrorOr<IEnumerable<InvestmentInstrument>>> GetAllAsync(UserId userId, CancellationToken cancellationToken)
	{
		AsyncPageable<TableEntity> entities = this.tableClient.QueryAsync<TableEntity>(
			entity => entity.PartitionKey == userId.Value.ToString("N"));

		List<InvestmentInstrument> instruments = new List<InvestmentInstrument>();

		await foreach (TableEntity entity in entities)
		{
			instruments.Add(this.MapToInstrument(entity));
		}

		return instruments;
	}

	public async Task<ErrorOr<InvestmentInstrument>> GetByIdAsync(UserId userId, InstrumentId instrumentId, CancellationToken cancellationToken)
	{
		try
		{
			TableEntity entity = await this.tableClient.GetEntityAsync<TableEntity>(
				userId.Value.ToString("N"),
				instrumentId.Value.ToString("N"));

			return this.MapToInstrument(entity);
		}
		catch (RequestFailedException ex) when (ex.Status == 404)
		{
			return Error.NotFound();
		}
		catch (Exception ex)
		{
			return Error.Failure(ex.Message);
		}
	}

	public async Task<ErrorOr<MutualFundInstrument>> AddMutualFundAsync(UserId userId, string name, InvestmentCategory category, MutualFundId mutualFundId, CancellationToken cancellationToken)
	{
		var mutualFundInstrument = new MutualFundInstrument(
			InstrumentId.New(),
			name,
			category,
			mutualFundId);

		MutualFundInstrumentTableEntity entity = new MutualFundInstrumentTableEntity
		{
			PartitionKey = userId.Value.ToString("N"),
			RowKey = mutualFundInstrument.Id.Value.ToString("N"),
			Name = name,
			Category = category,
			Type = mutualFundInstrument.Type,
			MutualFundId = mutualFundId.Value.ToString("N"),
		};

		try
		{
			await this.tableClient.AddEntityAsync(entity, cancellationToken);

			return mutualFundInstrument;
		}
		catch (RequestFailedException ex) when (ex.Status == 409)
		{
			return Error.Conflict();
		}
		catch (Exception ex)
		{
			return Error.Failure(ex.Message);
		}
	}

	private InvestmentInstrument MapToInstrument(TableEntity entity)
	{
		var type = Enum.Parse<InvestmentType>(entity["Type"].ToString());

		return type switch
		{
			InvestmentType.MutualFunds => new MutualFundInstrument(
				new InstrumentId(Guid.Parse(entity.RowKey)),
				entity["Name"].ToString(),
				Enum.Parse<InvestmentCategory>(entity["Category"].ToString()),
				new MutualFundId(Guid.Parse(entity["MutualFundId"].ToString()))),
			_ => throw new NotSupportedException($"Investment type '{type}' is not supported."),
		};
	}

	private sealed class MutualFundInstrumentTableEntity : InstrumentTableEntity
	{
		public string MutualFundId { get; set; }
	}

	private abstract class InstrumentTableEntity : ITableEntity
	{
		public string PartitionKey { get; set; }

		public string RowKey { get; set; }

		public DateTimeOffset? Timestamp { get; set; }

		public ETag ETag { get; set; }

		public string Name { get; set; }

		public InvestmentCategory Category { get; set; }

		public InvestmentType Type { get; set; }
	}
}
