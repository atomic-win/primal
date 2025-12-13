using Primal.Domain.Money;

namespace Primal.Application.Investments;

public interface IStockApiClient : IAssetApiClient<Stock>
{
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0048:File name must match type name", Justification = "cohesion")]
public sealed record Stock(
	string Symbol,
	string Name,
	Currency Currency);
