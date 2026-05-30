using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public interface ITransactionAmountCalculator
{
	Task<decimal> CalculateAmountAsync(
		UserId userId,
		Transaction transaction,
		DateOnly date,
		Currency targetCurrency,
		CancellationToken cancellationToken);
}
