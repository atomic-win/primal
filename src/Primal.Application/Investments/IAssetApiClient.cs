namespace Primal.Application.Investments;

public interface IAssetApiClient<T>
{
	Task<T> GetByIdAsync(
		string id,
		CancellationToken cancellationToken);

	Task<IReadOnlyDictionary<DateOnly, decimal>> GetPricesAsync(
		string id,
		CancellationToken cancellationToken);

	Task<decimal> GetOnOrBeforePriceAsync(
		string id,
		DateOnly date,
		CancellationToken cancellationToken);
}
