using Primal.Domain.Money;

namespace Primal.Application.Investments;

public interface IExchangeRateProvider
{
	Task<IReadOnlyDictionary<DateOnly, decimal>> GetExchangeRatesAsync(
		Currency from,
		Currency to,
		CancellationToken cancellationToken);
}
