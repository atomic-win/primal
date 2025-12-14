namespace Primal.Application.Investments;

public interface IAssetApiClient<T>
{
	Task<T> GetBySymbolAsync(
		string symbol,
		CancellationToken cancellationToken);

	Task<IReadOnlyDictionary<DateOnly, decimal>> GetPricesAsync(
		string symbol,
		CancellationToken cancellationToken);

	Task<decimal> GetOnOrBeforePriceAsync(
		string symbol,
		DateOnly date,
		CancellationToken cancellationToken);
}
