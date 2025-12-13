using Primal.Domain.Money;

namespace Primal.Application.Investments;

public interface IStockApiClient
{
	Task<Stock> GetBySymbolAsync(string symbol, CancellationToken cancellationToken);

	Task<IReadOnlyDictionary<DateOnly, decimal>> GetPriceAsync(string symbol, CancellationToken cancellationToken);
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0048:File name must match type name", Justification = "cohesion")]
public sealed record Stock(
	string Symbol,
	string Name,
	Currency Currency);
