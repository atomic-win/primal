using ErrorOr;
using Primal.Domain.Money;

namespace Primal.Application.Common.Interfaces.Investments;

public interface IExchangeRateProvider
{
	Task<ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>> GetExchangeRatesAsync(
		Currency from,
		Currency to,
		CancellationToken cancellationToken);
}
