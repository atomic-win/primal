using Primal.Domain.Money;

namespace Primal.Domain.Investments;

public sealed class Stock : InvestmentInstrument
{
	public Stock(
		InstrumentId id,
		string name,
		string symbol,
		string stockType,
		string region,
		string marketOpen,
		string marketClose,
		string timezone,
		Currency currency)
		: base(id, name, InstrumentType.Stocks, currency)
	{
		this.Symbol = symbol;
		this.StockType = stockType;
		this.Region = region;
		this.MarketOpen = marketOpen;
		this.MarketClose = marketClose;
		this.Timezone = timezone;
	}

	public string Symbol { get; init; }

	public string StockType { get; init; }

	public string Region { get; init; }

	public string MarketOpen { get; init; }

	public string MarketClose { get; init; }

	public string Timezone { get; init; }
}
