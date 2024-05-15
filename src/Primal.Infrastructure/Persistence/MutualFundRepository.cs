using System.Globalization;
using Azure;
using Azure.Data.Tables;
using ErrorOr;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Infrastructure.Persistence;

internal sealed class MutualFundRepository : IMutualFundRepository
{
	private readonly TableClient idMapTableClient;
	private readonly TableClient mutualFundTableClient;

	internal MutualFundRepository(TableClient idMapTableClient, TableClient mutualFundTableClient)
	{
		this.idMapTableClient = idMapTableClient;
		this.mutualFundTableClient = mutualFundTableClient;
	}

	public async Task<ErrorOr<MutualFund>> GetBySchemeCodeAsync(int schemeCode, CancellationToken cancellationToken)
	{
		try
		{
			MutualFundSchemeCodeTableEntity entity = await this.idMapTableClient.GetEntityAsync<MutualFundSchemeCodeTableEntity>(
				"MutualFundSchemeCode",
				schemeCode.ToString(CultureInfo.InvariantCulture),
				cancellationToken: cancellationToken);

			return await this.GetByIdAsync(new MutualFundId(Guid.Parse(entity.MutualFundId)), cancellationToken);
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

	public async Task<ErrorOr<MutualFund>> GetByIdAsync(MutualFundId mutualFundId, CancellationToken cancellationToken)
	{
		try
		{
			MutualFundTableEntity entity = await this.mutualFundTableClient.GetEntityAsync<MutualFundTableEntity>(
				mutualFundId.Value.ToString("N"),
				"MutualFund",
				cancellationToken: cancellationToken);

			return new MutualFund(
				mutualFundId,
				entity.SchemeName,
				entity.FundHouse,
				entity.SchemeType,
				entity.SchemeCategory,
				entity.SchemeCode,
				entity.Currency);
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

	public async Task<ErrorOr<MutualFund>> AddAsync(string schemeName, string fundHouse, string schemeType, string schemeCategory, int schemeCode, Currency currency, CancellationToken cancellationToken)
	{
		try
		{
			var mutualFundId = MutualFundId.New();

			var mutualFundSchemeCodeEntity = new MutualFundSchemeCodeTableEntity
			{
				RowKey = schemeCode.ToString(CultureInfo.InvariantCulture),
				MutualFundId = mutualFundId.Value.ToString("N"),
			};

			var mutualFundEntity = new MutualFundTableEntity
			{
				PartitionKey = mutualFundId.Value.ToString("N"),
				SchemeName = schemeName,
				FundHouse = fundHouse,
				SchemeType = schemeType,
				SchemeCategory = schemeCategory,
				SchemeCode = schemeCode,
				Currency = currency,
			};

			await this.idMapTableClient.AddEntityAsync(mutualFundSchemeCodeEntity, cancellationToken: cancellationToken);
			await this.mutualFundTableClient.AddEntityAsync(mutualFundEntity, cancellationToken: cancellationToken);

			return new MutualFund(
				mutualFundId,
				schemeName,
				fundHouse,
				schemeType,
				schemeCategory,
				schemeCode,
				currency);
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

	private sealed class MutualFundSchemeCodeTableEntity : ITableEntity
	{
		public string PartitionKey { get; set; } = "MutualFundSchemeCode";

		public string RowKey { get; set; } = string.Empty;

		public DateTimeOffset? Timestamp { get; set; }

		public ETag ETag { get; set; }

		public string MutualFundId { get; set; } = string.Empty;
	}

	private sealed class MutualFundTableEntity : ITableEntity
	{
		public string PartitionKey { get; set; } = string.Empty;

		public string RowKey { get; set; } = "MutualFund";

		public DateTimeOffset? Timestamp { get; set; }

		public ETag ETag { get; set; }

		public string SchemeName { get; set; } = string.Empty;

		public string FundHouse { get; set; } = string.Empty;

		public string SchemeType { get; set; } = string.Empty;

		public string SchemeCategory { get; set; } = string.Empty;

		public int SchemeCode { get; set; }

		public Currency Currency { get; set; } = Currency.Unknown;
	}
}
