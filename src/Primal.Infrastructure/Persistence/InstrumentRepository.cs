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

	public async Task<ErrorOr<IEnumerable<Instrument>>> GetAllAsync(UserId userId, CancellationToken cancellationToken)
	{
		AsyncPageable<InstrumentTableEntity> entities = this.tableClient.QueryAsync<InstrumentTableEntity>(
			entity => entity.PartitionKey == userId.Value.ToString("N"));

		List<Instrument> instruments = new List<Instrument>();

		await foreach (InstrumentTableEntity entity in entities)
		{
			instruments.Add(
				new Instrument(
					new InstrumentId(Guid.Parse(entity.RowKey)),
					entity.Name,
					entity.Category,
					entity.Type));
		}

		return instruments;
	}

	public async Task<ErrorOr<Instrument>> GetByIdAsync(UserId userId, InstrumentId instrumentId, CancellationToken cancellationToken)
	{
		InstrumentTableEntity entity = await this.tableClient.GetEntityAsync<InstrumentTableEntity>(
			userId.Value.ToString("N"),
			instrumentId.Value.ToString("N"));

		if (entity == null)
		{
			return Error.NotFound();
		}

		return new Instrument(
			new InstrumentId(Guid.Parse(entity.RowKey)),
			entity.Name,
			entity.Category,
			entity.Type);
	}

	public async Task<ErrorOr<Instrument>> AddAsync(UserId userId, string name, InvestmentCategory category, InvestmentType type, CancellationToken cancellationToken)
	{
		var instrumentId = InstrumentId.New();

		var instrument = new InstrumentTableEntity
		{
			PartitionKey = userId.Value.ToString("N"),
			RowKey = instrumentId.Value.ToString("N"),
			Name = name,
			Category = category,
			Type = type,
		};

		try
		{
			await this.tableClient.AddEntityAsync(instrument);
			return new Instrument(instrumentId, name, category, type);
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

	private sealed class InstrumentTableEntity : ITableEntity
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
