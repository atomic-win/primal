using Primal.Domain.Investments;

namespace Primal.Api.Transactions;

internal static class TransactionValidationExtensions
{
	internal static bool IsValidForAssetType(
		TransactionType transactionType,
		Asset asset)
	{
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

	internal static bool IsUnitsRequired(TransactionType transactionType)
	{
		return transactionType switch
		{
			TransactionType.Buy or TransactionType.Sell => true,
			_ => false,
		};
	}
}
