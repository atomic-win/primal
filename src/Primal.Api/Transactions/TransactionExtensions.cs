using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.Transactions;

internal static class TransactionExtensions
{
	internal static async Task<TransactionResponse> ToResponse(
		this Transaction transaction,
		UserId userId,
		ITransactionAmountCalculator transactionAmountCalculator,
		Currency targetCurrency,
		CancellationToken cancellationToken)
	{
		var amount = await transactionAmountCalculator.CalculateAmountAsync(
			userId,
			transaction,
			transaction.Date,
			targetCurrency,
			cancellationToken);

		return new TransactionResponse(
			transaction.Id.Value,
			transaction.Date,
			transaction.Name,
			transaction.TransactionType,
			transaction.AssetItemId.Value,
			transaction.Units,
			transaction.Price,
			amount);
	}
}
