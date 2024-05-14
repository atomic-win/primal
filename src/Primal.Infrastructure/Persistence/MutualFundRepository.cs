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
	private readonly TableClient schemeCodeTableClient;
	private readonly TableClient mutualFundTableClient;

	internal MutualFundRepository(TableClient schemeCodeTableClient, TableClient mutualFundTableClient)
	{
		this.schemeCodeTableClient = schemeCodeTableClient;
		this.mutualFundTableClient = mutualFundTableClient;
	}

	public async Task<ErrorOr<MutualFund>> GetBySchemeCodeAsync(int schemeCode, CancellationToken cancellationToken)
	{
		try
		{
			MutualFundSchemeCodeTableEntity entity = await this.schemeCodeTableClient.GetEntityAsync<MutualFundSchemeCodeTableEntity>(
				schemeCode.ToString(CultureInfo.InvariantCulture),
				"SchemeCode",
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
		MutualFundTableEntity entity = await this.mutualFundTableClient.GetEntityAsync<MutualFundTableEntity>(
			mutualFundId.Value.ToString("N"),
			"MutualFund",
			cancellationToken: cancellationToken);

		if (entity == null)
		{
			return Error.NotFound();
		}

		return new MutualFund(
			mutualFundId,
			entity.SchemeName,
			entity.FundHouse,
			entity.SchemeType,
			entity.SchemeCategory,
			entity.SchemeCode,
			entity.Currency);
	}

	public async Task<ErrorOr<MutualFund>> AddAsync(string schemeName, string fundHouse, string schemeType, string schemeCategory, int schemeCode, Currency currency, CancellationToken cancellationToken)
	{
		try
		{
			var mutualFundId = MutualFundId.New();

			await this.schemeCodeTableClient.AddEntityAsync(
				new MutualFundSchemeCodeTableEntity
				{
					PartitionKey = schemeCode.ToString(CultureInfo.InvariantCulture),
					MutualFundId = mutualFundId.Value.ToString("N"),
				},
				cancellationToken: cancellationToken);

			await this.mutualFundTableClient.AddEntityAsync(
				new MutualFundTableEntity
				{
					PartitionKey = mutualFundId.Value.ToString("N"),
					SchemeName = schemeName,
					FundHouse = fundHouse,
					SchemeType = schemeType,
					SchemeCategory = schemeCategory,
					SchemeCode = schemeCode,
					Currency = currency,
				},
				cancellationToken: cancellationToken);

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
		public string PartitionKey { get; set; } = string.Empty;

		public string RowKey { get; set; } = "SchemeCode";

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
