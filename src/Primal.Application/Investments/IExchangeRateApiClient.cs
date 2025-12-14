using Primal.Domain.Money;

namespace Primal.Application.Investments;

public interface IExchangeRateApiClient
{
	Task<IReadOnlyDictionary<DateOnly, decimal>> GetExchangeRatesAsync(
		Currency from,
		Currency to,
		CancellationToken cancellationToken);

	Task<decimal> GetOnOrBeforeExchangeRateAsync(
		Currency from,
		Currency to,
		DateOnly date,
		CancellationToken cancellationToken);
}
