using Primal.Domain.Common.Models;
using Primal.Domain.Money;

namespace Primal.Domain.Investments;

public sealed class Stock : Entity<StockId>
{
	public Stock(StockId id, string symbol, string name, string region, Currency currency)
		: base(id)
	{
		this.Symbol = symbol;
		this.Name = name;
		this.Region = region;
		this.Currency = currency;
	}

	public string Symbol { get; init; }

	public string Name { get; init; }

	public string Region { get; init; }

	public Currency Currency { get; init; }
}
