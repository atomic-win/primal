using Primal.Domain.Money;

namespace Primal.Application.Investments;

public interface IForexApiClient
{
	Task<IReadOnlyDictionary<DateOnly, decimal>> GetForexRatesAsync(
		Currency from,
		Currency to,
		CancellationToken cancellationToken);

	Task<decimal> GetOnOrBeforeForexRateAsync(
		Currency from,
		Currency to,
		DateOnly date,
		CancellationToken cancellationToken);
}
