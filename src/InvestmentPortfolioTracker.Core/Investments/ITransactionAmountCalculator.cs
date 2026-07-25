using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Money;
using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Core.Investments;

public interface ITransactionAmountCalculator
{
	Task<decimal> CalculateAmountAsync(
		UserId userId,
		Transaction transaction,
		DateOnly date,
		Currency targetCurrency,
		CancellationToken cancellationToken);
}
