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
		TransactionAmountCalculator transactionAmountCalculator,
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
			amount);
	}

	internal static bool IsValidForAssetType(
		this TransactionRequest req,
		Asset asset)
	{
		var transactionType = req.TransactionType;

		return asset.AssetType switch
		{
			AssetType.BankAccount or
			AssetType.FixedDeposit or
			AssetType.EPF or
			AssetType.PPF =>
				transactionType == TransactionType.Deposit ||
				transactionType == TransactionType.Withdrawal ||
				transactionType == TransactionType.Interest ||
				transactionType == TransactionType.SelfInterest ||
				transactionType == TransactionType.InterestPenalty,
			AssetType.MutualFund =>
				transactionType == TransactionType.Buy ||
				transactionType == TransactionType.Sell,
			AssetType.Stock =>
				transactionType == TransactionType.Buy ||
				transactionType == TransactionType.Sell ||
				transactionType == TransactionType.Dividend,
			AssetType.Wallet or AssetType.TradingAccount =>
				transactionType == TransactionType.Deposit ||
				transactionType == TransactionType.Withdrawal,
			AssetType.Bond =>
				transactionType == TransactionType.Deposit ||
				transactionType == TransactionType.Withdrawal ||
				transactionType == TransactionType.Interest,
			_ => throw new InvalidOperationException(
					$"Unsupported asset type: {asset.AssetType}"),
		};
	}

	internal static bool IsUnitsRequired(this TransactionRequest req)
	{
		return req.TransactionType switch
		{
			TransactionType.Buy or TransactionType.Sell => true,
			_ => false,
		};
	}
}
