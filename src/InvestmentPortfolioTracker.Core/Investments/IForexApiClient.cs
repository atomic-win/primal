using InvestmentPortfolioTracker.Domain.Money;

namespace InvestmentPortfolioTracker.Core.Investments;

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
