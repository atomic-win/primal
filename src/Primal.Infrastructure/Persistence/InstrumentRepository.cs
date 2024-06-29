using ErrorOr;
using LiteDB;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Infrastructure.Persistence;

internal sealed class InstrumentRepository : IInstrumentRepository
{
	private readonly LiteDatabase liteDatabase;

	internal InstrumentRepository(LiteDatabase liteDatabase)
	{
		this.liteDatabase = liteDatabase;

		var collection = this.liteDatabase.GetCollection<InstrumentTableEntity>("Instruments");
		collection.EnsureIndex(x => x.Id, unique: true);
		collection.EnsureIndex(x => x.InstrumentType);
		collection.EnsureIndex(x => x.Currency);

		var mutualFundCollection = this.liteDatabase.GetCollection<MutualFundTableEntity>("Instruments");
		mutualFundCollection.EnsureIndex(x => x.SchemeCode);

		var stockCollection = this.liteDatabase.GetCollection<StockTableEntity>("Instruments");
		stockCollection.EnsureIndex(x => x.Symbol);
	}

	public async Task<ErrorOr<IEnumerable<InvestmentInstrument>>> GetAllAsync(CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var cashInstrumentCollection = this.liteDatabase.GetCollection<CashInstrumentTableEntity>("Instruments");
		var mutualFundCollection = this.liteDatabase.GetCollection<MutualFundTableEntity>("Instruments");
		var stockCollection = this.liteDatabase.GetCollection<StockTableEntity>("Instruments");

		return cashInstrumentCollection
			.FindAll()
			.Select(x => this.MapToInstrument(x))
			.Concat(mutualFundCollection
				.FindAll()
				.Select(this.MapToInstrument))
			.Concat(stockCollection
				.FindAll()
				.Select(this.MapToInstrument))
			.ToList();
	}

	public async Task<ErrorOr<InvestmentInstrument>> GetByIdAsync(
		InstrumentId instrumentId,
		CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var instrumentCollection = this.liteDatabase.GetCollection("Instruments");
		var document = instrumentCollection.FindById(instrumentId.Value);

		if (document == null)
		{
			return Error.NotFound(description: "Instrument does not exist");
		}

		return this.MapToInstrument(document);
	}

	public async Task<ErrorOr<InvestmentInstrument>> GetCashInstrumentAsync(
		InstrumentType instrumentType,
		Currency currency,
		CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var cashInstrumentCollection = this.liteDatabase.GetCollection<CashInstrumentTableEntity>("Instruments");

		var cashInstrumentTableEntity = cashInstrumentCollection.FindOne(x => x.InstrumentType == instrumentType && x.Currency == currency);

		if (cashInstrumentTableEntity == null)
		{
			return Error.NotFound(description: "Cash instrument does not exist");
		}

		return this.MapToInstrument(cashInstrumentTableEntity);
	}

	public async Task<ErrorOr<InvestmentInstrument>> GetMutualFundBySchemeCodeAsync(
		int schemeCode,
		CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var mutualFundCollection = this.liteDatabase.GetCollection<MutualFundTableEntity>("Instruments");

		var mutualFundTableEntity = mutualFundCollection.FindOne(x => x.InstrumentType == InstrumentType.MutualFunds && x.SchemeCode == schemeCode);

		if (mutualFundTableEntity == null)
		{
			return Error.NotFound(description: "Mutual fund does not exist");
		}

		return this.MapToInstrument(mutualFundTableEntity);
	}

	public async Task<ErrorOr<InvestmentInstrument>> GetStockBySymbolAsync(
		string symbol,
		CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var stockCollection = this.liteDatabase.GetCollection<StockTableEntity>("Instruments");

		var stockTableEntity = stockCollection.FindOne(x => x.InstrumentType == InstrumentType.Stocks && x.Symbol == symbol);

		if (stockTableEntity == null)
		{
			return Error.NotFound(description: "Stock does not exist");
		}

		return this.MapToInstrument(stockTableEntity);
	}

	public async Task<ErrorOr<InvestmentInstrument>> AddCashInstrumentAsync(
		InstrumentType instrumentType,
		Currency currency,
		CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var cashInstrumentCollection = this.liteDatabase.GetCollection<CashInstrumentTableEntity>("Instruments");

		var cashInstrumentTableEntity = new CashInstrumentTableEntity
		{
			Id = InstrumentId.New().Value,
			InstrumentType = instrumentType,
			Currency = currency,
		};

		cashInstrumentCollection.Insert(cashInstrumentTableEntity);

		return this.MapToInstrument(cashInstrumentTableEntity);
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
		await Task.CompletedTask;

		var mutualFundCollection = this.liteDatabase.GetCollection<MutualFundTableEntity>("Instruments");

		var mutualFundTableEntity = new MutualFundTableEntity
		{
			Id = InstrumentId.New().Value,
			Name = name,
			FundHouse = fundHouse,
			SchemeType = schemeType,
			SchemeCategory = schemeCategory,
			SchemeCode = schemeCode,
			InstrumentType = InstrumentType.MutualFunds,
			Currency = currency,
		};

		mutualFundCollection.Insert(mutualFundTableEntity);

		return this.MapToInstrument(mutualFundTableEntity);
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
		await Task.CompletedTask;

		var stockCollection = this.liteDatabase.GetCollection<StockTableEntity>("Instruments");

		var stockTableEntity = new StockTableEntity
		{
			Id = InstrumentId.New().Value,
			Name = name,
			Symbol = symbol,
			StockType = stockType,
			Region = region,
			MarketOpen = marketOpen,
			MarketClose = marketClose,
			Timezone = timezone,
			InstrumentType = InstrumentType.Stocks,
			Currency = currency,
		};

		stockCollection.Insert(stockTableEntity);

		return this.MapToInstrument(stockTableEntity);
	}

	private ErrorOr<InvestmentInstrument> MapToInstrument(BsonDocument bsonDocument)
	{
		var instrumentId = new InstrumentId(bsonDocument["_id"].AsGuid);
		var instrumentType = Enum.Parse<InstrumentType>(bsonDocument["InstrumentType"].AsString);
		var currency = Enum.Parse<Currency>(bsonDocument["Currency"].AsString);

		switch (instrumentType)
		{
			case InstrumentType.CashAccounts:
			case InstrumentType.FixedDeposits:
			case InstrumentType.EPF:
			case InstrumentType.PPF:
				return new CashInstrument(
					instrumentId,
					instrumentType,
					currency);
			case InstrumentType.MutualFunds:
				return new MutualFund(
					instrumentId,
					bsonDocument["Name"].AsString,
					bsonDocument["FundHouse"].AsString,
					bsonDocument["SchemeType"].AsString,
					bsonDocument["SchemeCategory"].AsString,
					bsonDocument["SchemeCode"].AsInt32,
					currency);
			case InstrumentType.Stocks:
				return new Stock(
					instrumentId,
					bsonDocument["Name"].AsString,
					bsonDocument["Symbol"].AsString,
					bsonDocument["StockType"].AsString,
					bsonDocument["Region"].AsString,
					bsonDocument["MarketOpen"].AsString,
					bsonDocument["MarketClose"].AsString,
					bsonDocument["Timezone"].AsString,
					currency);
			default:
				return Error.Validation(description: "Invalid instrument type");
		}
	}

	private InvestmentInstrument MapToInstrument(CashInstrumentTableEntity cashInstrumentTableEntity)
	{
		return new CashInstrument(
			new InstrumentId(cashInstrumentTableEntity.Id),
			cashInstrumentTableEntity.InstrumentType,
			cashInstrumentTableEntity.Currency);
	}

	private InvestmentInstrument MapToInstrument(MutualFundTableEntity mutualFundTableEntity)
	{
		return new MutualFund(
			new InstrumentId(mutualFundTableEntity.Id),
			mutualFundTableEntity.Name,
			mutualFundTableEntity.FundHouse,
			mutualFundTableEntity.SchemeType,
			mutualFundTableEntity.SchemeCategory,
			mutualFundTableEntity.SchemeCode,
			mutualFundTableEntity.Currency);
	}

	private InvestmentInstrument MapToInstrument(StockTableEntity stockTableEntity)
	{
		return new Stock(
			new InstrumentId(stockTableEntity.Id),
			stockTableEntity.Name,
			stockTableEntity.Symbol,
			stockTableEntity.StockType,
			stockTableEntity.Region,
			stockTableEntity.MarketOpen,
			stockTableEntity.MarketClose,
			stockTableEntity.Timezone,
			stockTableEntity.Currency);
	}

	private abstract class InstrumentTableEntity
	{
		public Guid Id { get; set; }

		public string Name { get; set; }

		public InstrumentType InstrumentType { get; set; }

		public Currency Currency { get; set; }
	}

	private sealed class CashInstrumentTableEntity : InstrumentTableEntity
	{
	}

	private sealed class MutualFundTableEntity : InstrumentTableEntity
	{
		public string FundHouse { get; set; }

		public string SchemeType { get; set; }

		public string SchemeCategory { get; set; }

		public int SchemeCode { get; set; }
	}

	private sealed class StockTableEntity : InstrumentTableEntity
	{
		public string Symbol { get; set; }

		public string StockType { get; set; }

		public string Region { get; set; }

		public string MarketOpen { get; set; }

		public string MarketClose { get; set; }

		public string Timezone { get; set; }
	}
}
