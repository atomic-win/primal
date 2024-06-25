using System.Globalization;
using Azure;
using Azure.Data.Tables;
using ErrorOr;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Infrastructure.Persistence;

internal sealed class InstrumentRepository : IInstrumentRepository
{
	private readonly TableClient instrumentIdMappingTableClient;
	private readonly TableClient instrumentTableClient;

	internal InstrumentRepository(
		TableClient instrumentIdMappingTableClient,
		TableClient instrumentTableClient)
	{
		this.instrumentIdMappingTableClient = instrumentIdMappingTableClient;
		this.instrumentTableClient = instrumentTableClient;
	}

	public async Task<ErrorOr<IEnumerable<InvestmentInstrument>>> GetAllAsync(CancellationToken cancellationToken)
	{
		try
		{
			List<InvestmentInstrument> instruments = new List<InvestmentInstrument>();

			await foreach (TableEntity entity in this.instrumentTableClient.QueryAsync<TableEntity>(cancellationToken: cancellationToken))
			{
				instruments.Add(this.MapToInstrument(entity));
			}

			return instruments;
		}
		catch (Exception ex)
		{
			return Error.Failure(description: ex.Message);
		}
	}

	public async Task<ErrorOr<InvestmentInstrument>> GetByIdAsync(InstrumentId instrumentId, CancellationToken cancellationToken)
	{
		try
		{
			TableEntity entity = await this.instrumentTableClient.GetEntityAsync<TableEntity>(
				partitionKey: instrumentId.Value.ToString("N"),
				rowKey: string.Empty);

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

	public async Task<ErrorOr<InvestmentInstrument>> GetCashInstrumentAsync(
		InstrumentType instrumentType,
		Currency currency,
		CancellationToken cancellationToken)
	{
		try
		{
			InstrumentIdMappingTableEntity instrumentIdMappingEntity = await this.instrumentIdMappingTableClient.GetEntityAsync<InstrumentIdMappingTableEntity>(
				partitionKey: instrumentType.ToString(),
				rowKey: currency.ToString(),
				cancellationToken: cancellationToken);

			return await this.GetByIdAsync(
				new InstrumentId(Guid.Parse(instrumentIdMappingEntity.InstrumentId)),
				cancellationToken);
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

	public async Task<ErrorOr<InvestmentInstrument>> GetMutualFundBySchemeCodeAsync(int schemeCode, CancellationToken cancellationToken)
	{
		try
		{
			InstrumentIdMappingTableEntity instrumentIdMappingEntity = await this.instrumentIdMappingTableClient.GetEntityAsync<InstrumentIdMappingTableEntity>(
				partitionKey: "MutualFundSchemeCode",
				rowKey: schemeCode.ToString(CultureInfo.InvariantCulture),
				cancellationToken: cancellationToken);

			return await this.GetByIdAsync(
				new InstrumentId(Guid.Parse(instrumentIdMappingEntity.InstrumentId)),
				cancellationToken);
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

	public async Task<ErrorOr<InvestmentInstrument>> GetStockBySymbolAsync(string symbol, CancellationToken cancellationToken)
	{
		try
		{
			InstrumentIdMappingTableEntity instrumentIdMappingEntity = await this.instrumentIdMappingTableClient.GetEntityAsync<InstrumentIdMappingTableEntity>(
				partitionKey: "StockSymbol",
				rowKey: symbol.ToUpperInvariant(),
				cancellationToken: cancellationToken);

			return await this.GetByIdAsync(
				new InstrumentId(Guid.Parse(instrumentIdMappingEntity.InstrumentId)),
				cancellationToken);
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

	public async Task<ErrorOr<InvestmentInstrument>> AddCashInstrumentAsync(
		InstrumentType instrumentType,
		Currency currency,
		CancellationToken cancellationToken)
	{
		var cashInstrument = new CashInstrument(InstrumentId.New(), instrumentType, currency);

		InstrumentIdMappingTableEntity mappingEntity = new InstrumentIdMappingTableEntity
		{
			PartitionKey = instrumentType.ToString(),
			RowKey = currency.ToString(),
			InstrumentId = cashInstrument.Id.Value.ToString("N"),
		};

		var entity = new CashInstrumentTableEntity
		{
			PartitionKey = cashInstrument.Id.Value.ToString("N"),
			Name = cashInstrument.Name,
			Type = cashInstrument.Type.ToString(),
			Currency = cashInstrument.Currency,
		};

		try
		{
			await this.instrumentIdMappingTableClient.AddEntityAsync(mappingEntity, cancellationToken);
			await this.instrumentTableClient.AddEntityAsync(entity, cancellationToken);
			return cashInstrument;
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

	public async Task<ErrorOr<InvestmentInstrument>> AddMutualFundAsync(
		string name,
		string fundHouse,
		string schemeType,
		string schemeCategory,
		int schemeCode,
		Currency currency,
		CancellationToken cancellationToken)
	{
		var mutualFund = new MutualFund(
			InstrumentId.New(),
			name,
			fundHouse,
			schemeType,
			schemeCategory,
			schemeCode,
			currency);

		InstrumentIdMappingTableEntity mappingEntity = new InstrumentIdMappingTableEntity
		{
			PartitionKey = "MutualFundSchemeCode",
			RowKey = schemeCode.ToString(CultureInfo.InvariantCulture),
			InstrumentId = mutualFund.Id.Value.ToString("N"),
		};

		MutualFundInstrumentTableEntity entity = new MutualFundInstrumentTableEntity
		{
			PartitionKey = mutualFund.Id.Value.ToString("N"),
			Name = mutualFund.Name,
			Type = mutualFund.Type.ToString(),
			FundHouse = mutualFund.FundHouse,
			SchemeType = mutualFund.SchemeType,
			SchemeCategory = mutualFund.SchemeCategory,
			SchemeCode = mutualFund.SchemeCode,
			Currency = mutualFund.Currency,
		};

		try
		{
			await this.instrumentIdMappingTableClient.AddEntityAsync(mappingEntity, cancellationToken);
			await this.instrumentTableClient.AddEntityAsync(entity, cancellationToken);
			return mutualFund;
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

	public async Task<ErrorOr<InvestmentInstrument>> AddStockAsync(
		string name,
		string symbol,
		string stockType,
		string region,
		string marketOpen,
		string marketClose,
		string timezone,
		Currency currency,
		CancellationToken cancellationToken)
	{
		var stock = new Stock(
			InstrumentId.New(),
			name,
			symbol,
			stockType,
			region,
			marketOpen,
			marketClose,
			timezone,
			currency);

		InstrumentIdMappingTableEntity mappingEntity = new InstrumentIdMappingTableEntity
		{
			PartitionKey = "StockSymbol",
			RowKey = stock.Symbol.ToUpperInvariant(),
			InstrumentId = stock.Id.Value.ToString("N"),
		};

		StockInstrumentTableEntity entity = new StockInstrumentTableEntity
		{
			PartitionKey = stock.Id.Value.ToString("N"),
			Name = stock.Name,
			Type = stock.Type.ToString(),
			Symbol = stock.Symbol,
			StockType = stock.StockType,
			Region = stock.Region,
			MarketOpen = stock.MarketOpen,
			MarketClose = stock.MarketClose,
			Timezone = stock.Timezone,
			Currency = stock.Currency,
		};

		try
		{
			await this.instrumentIdMappingTableClient.AddEntityAsync(mappingEntity, cancellationToken);
			await this.instrumentTableClient.AddEntityAsync(entity, cancellationToken);
			return stock;
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
		var type = Enum.Parse<InstrumentType>(entity.GetString("Type"));

		return type switch
		{
			InstrumentType.CashAccounts or InstrumentType.FixedDeposits or InstrumentType.EPF or InstrumentType.PPF => new CashInstrument(
				new InstrumentId(Guid.Parse(entity.PartitionKey)),
				type,
				Enum.Parse<Currency>(entity.GetString("Currency"))),

			InstrumentType.MutualFunds => new MutualFund(
				new InstrumentId(Guid.Parse(entity.PartitionKey)),
				entity.GetString("Name"),
				entity.GetString("FundHouse"),
				entity.GetString("SchemeType"),
				entity.GetString("SchemeCategory"),
				(int)entity.GetInt32("SchemeCode"),
				Enum.Parse<Currency>(entity.GetString("Currency"))),

			InstrumentType.Stocks => new Stock(
				new InstrumentId(Guid.Parse(entity.PartitionKey)),
				entity.GetString("Name"),
				entity.GetString("Symbol"),
				entity.GetString("StockType"),
				entity.GetString("Region"),
				entity.GetString("MarketOpen"),
				entity.GetString("MarketClose"),
				entity.GetString("Timezone"),
				Enum.Parse<Currency>(entity.GetString("Currency"))),

			_ => throw new NotSupportedException($"Investment type '{type}' is not supported."),
		};
	}

	private sealed class CashInstrumentTableEntity : InstrumentTableEntity
	{
	}

	private sealed class MutualFundInstrumentTableEntity : InstrumentTableEntity
	{
		public string FundHouse { get; set; }

		public string SchemeType { get; set; }

		public string SchemeCategory { get; set; }

		public int SchemeCode { get; set; }
	}

	private sealed class StockInstrumentTableEntity : InstrumentTableEntity
	{
		public string Symbol { get; set; }

		public string StockType { get; set; }

		public string Region { get; set; }

		public string MarketOpen { get; set; }

		public string MarketClose { get; set; }

		public string Timezone { get; set; }
	}

	private abstract class InstrumentTableEntity : ITableEntity
	{
		public string PartitionKey { get; set; }

		public string RowKey { get; set; } = string.Empty;

		public DateTimeOffset? Timestamp { get; set; }

		public ETag ETag { get; set; }

		public string Name { get; set; }

		public string Type { get; set; }

		public Currency Currency { get; set; }
	}

	private sealed class InstrumentIdMappingTableEntity : ITableEntity
	{
		public string PartitionKey { get; set; }

		public string RowKey { get; set; }

		public DateTimeOffset? Timestamp { get; set; }

		public ETag ETag { get; set; }

		public string InstrumentId { get; set; }
	}
}
