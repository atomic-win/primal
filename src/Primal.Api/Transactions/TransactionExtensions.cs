using Primal.Domain.Investments;

namespace Primal.Api.Transactions;

internal static class TransactionExtensions
{
	internal static TransactionResponse ToResponse(this Transaction transaction)
	{
		return new TransactionResponse(
			transaction.Id.Value,
			transaction.Date,
			transaction.Name,
			transaction.TransactionType,
			transaction.AssetItemId.Value,
			transaction.Units,
			Amount: 0m);
	}

	internal static bool IsValidForAssetType(
		this TransactionType transactionType,
		AssetType assetType)
	{
		return assetType switch
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
				transactionType == TransactionType.Buy ||
				transactionType == TransactionType.Sell ||
				transactionType == TransactionType.Interest,
			_ => throw new InvalidOperationException(
					$"Unsupported asset type: {assetType}"),
		};
	}
}
