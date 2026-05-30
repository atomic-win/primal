using Primal.Api.Transactions;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Api.UnitTests.Api.Transactions;

public sealed class TransactionValidationExtensionsTests
{
	[Test]
	[Arguments(TransactionType.Buy)]
	[Arguments(TransactionType.Sell)]
	public async Task IsUnitsRequired_ReturnsTrue_ForBuyAndSell(TransactionType transactionType)
	{
		var result = TransactionValidationExtensions.IsUnitsRequired(transactionType);

		await Assert.That(result).IsTrue();
	}

	[Test]
	[Arguments(TransactionType.Deposit)]
	[Arguments(TransactionType.Withdrawal)]
	[Arguments(TransactionType.Interest)]
	[Arguments(TransactionType.SelfInterest)]
	[Arguments(TransactionType.InterestPenalty)]
	[Arguments(TransactionType.Dividend)]
	[Arguments(TransactionType.Unknown)]
	public async Task IsUnitsRequired_ReturnsFalse_ForNonBuySellTypes(TransactionType transactionType)
	{
		var result = TransactionValidationExtensions.IsUnitsRequired(transactionType);

		await Assert.That(result).IsFalse();
	}

	[Test]
	[Arguments(TransactionType.Deposit)]
	[Arguments(TransactionType.Withdrawal)]
	[Arguments(TransactionType.Interest)]
	[Arguments(TransactionType.SelfInterest)]
	[Arguments(TransactionType.InterestPenalty)]
	public async Task IsValidForAssetType_BankAccount_AcceptsValidTypes(TransactionType transactionType)
	{
		var asset = CreateAsset(AssetType.BankAccount);

		var result = TransactionValidationExtensions.IsValidForAssetType(transactionType, asset);

		await Assert.That(result).IsTrue();
	}

	[Test]
	[Arguments(TransactionType.Buy)]
	[Arguments(TransactionType.Sell)]
	[Arguments(TransactionType.Dividend)]
	public async Task IsValidForAssetType_BankAccount_RejectsInvalidTypes(TransactionType transactionType)
	{
		var asset = CreateAsset(AssetType.BankAccount);

		var result = TransactionValidationExtensions.IsValidForAssetType(transactionType, asset);

		await Assert.That(result).IsFalse();
	}

	[Test]
	[Arguments(TransactionType.Buy)]
	[Arguments(TransactionType.Sell)]
	public async Task IsValidForAssetType_MutualFund_AcceptsValidTypes(TransactionType transactionType)
	{
		var asset = CreateAsset(AssetType.MutualFund);

		var result = TransactionValidationExtensions.IsValidForAssetType(transactionType, asset);

		await Assert.That(result).IsTrue();
	}

	[Test]
	[Arguments(TransactionType.Deposit)]
	[Arguments(TransactionType.Withdrawal)]
	[Arguments(TransactionType.Interest)]
	[Arguments(TransactionType.Dividend)]
	public async Task IsValidForAssetType_MutualFund_RejectsInvalidTypes(TransactionType transactionType)
	{
		var asset = CreateAsset(AssetType.MutualFund);

		var result = TransactionValidationExtensions.IsValidForAssetType(transactionType, asset);

		await Assert.That(result).IsFalse();
	}

	[Test]
	[Arguments(TransactionType.Buy)]
	[Arguments(TransactionType.Sell)]
	[Arguments(TransactionType.Dividend)]
	public async Task IsValidForAssetType_Stock_AcceptsValidTypes(TransactionType transactionType)
	{
		var asset = CreateAsset(AssetType.Stock);

		var result = TransactionValidationExtensions.IsValidForAssetType(transactionType, asset);

		await Assert.That(result).IsTrue();
	}

	[Test]
	[Arguments(TransactionType.Deposit)]
	[Arguments(TransactionType.Withdrawal)]
	[Arguments(TransactionType.Interest)]
	public async Task IsValidForAssetType_Stock_RejectsInvalidTypes(TransactionType transactionType)
	{
		var asset = CreateAsset(AssetType.Stock);

		var result = TransactionValidationExtensions.IsValidForAssetType(transactionType, asset);

		await Assert.That(result).IsFalse();
	}

	[Test]
	[Arguments(TransactionType.Deposit)]
	[Arguments(TransactionType.Withdrawal)]
	public async Task IsValidForAssetType_Wallet_AcceptsValidTypes(TransactionType transactionType)
	{
		var asset = CreateAsset(AssetType.Wallet);

		var result = TransactionValidationExtensions.IsValidForAssetType(transactionType, asset);

		await Assert.That(result).IsTrue();
	}

	[Test]
	[Arguments(TransactionType.Buy)]
	[Arguments(TransactionType.Sell)]
	[Arguments(TransactionType.Interest)]
	[Arguments(TransactionType.Dividend)]
	public async Task IsValidForAssetType_Wallet_RejectsInvalidTypes(TransactionType transactionType)
	{
		var asset = CreateAsset(AssetType.Wallet);

		var result = TransactionValidationExtensions.IsValidForAssetType(transactionType, asset);

		await Assert.That(result).IsFalse();
	}

	[Test]
	[Arguments(TransactionType.Deposit)]
	[Arguments(TransactionType.Withdrawal)]
	[Arguments(TransactionType.Interest)]
	public async Task IsValidForAssetType_Bond_AcceptsValidTypes(TransactionType transactionType)
	{
		var asset = CreateAsset(AssetType.Bond);

		var result = TransactionValidationExtensions.IsValidForAssetType(transactionType, asset);

		await Assert.That(result).IsTrue();
	}

	[Test]
	[Arguments(TransactionType.Buy)]
	[Arguments(TransactionType.Sell)]
	[Arguments(TransactionType.Dividend)]
	[Arguments(TransactionType.SelfInterest)]
	public async Task IsValidForAssetType_Bond_RejectsInvalidTypes(TransactionType transactionType)
	{
		var asset = CreateAsset(AssetType.Bond);

		var result = TransactionValidationExtensions.IsValidForAssetType(transactionType, asset);

		await Assert.That(result).IsFalse();
	}

	[Test]
	[Arguments(AssetType.FixedDeposit)]
	[Arguments(AssetType.EPF)]
	[Arguments(AssetType.PPF)]
	public async Task IsValidForAssetType_DepositBasedAssets_AcceptSameTypesAsBankAccount(AssetType assetType)
	{
		var asset = CreateAsset(assetType);

		var depositResult = TransactionValidationExtensions.IsValidForAssetType(TransactionType.Deposit, asset);
		var withdrawalResult = TransactionValidationExtensions.IsValidForAssetType(TransactionType.Withdrawal, asset);
		var interestResult = TransactionValidationExtensions.IsValidForAssetType(TransactionType.Interest, asset);
		var buyResult = TransactionValidationExtensions.IsValidForAssetType(TransactionType.Buy, asset);

		await Assert.That(depositResult).IsTrue();
		await Assert.That(withdrawalResult).IsTrue();
		await Assert.That(interestResult).IsTrue();
		await Assert.That(buyResult).IsFalse();
	}

	[Test]
	public async Task IsValidForAssetType_TradingAccount_BehavesSameAsWallet()
	{
		var asset = CreateAsset(AssetType.TradingAccount);

		var depositResult = TransactionValidationExtensions.IsValidForAssetType(TransactionType.Deposit, asset);
		var withdrawalResult = TransactionValidationExtensions.IsValidForAssetType(TransactionType.Withdrawal, asset);
		var buyResult = TransactionValidationExtensions.IsValidForAssetType(TransactionType.Buy, asset);

		await Assert.That(depositResult).IsTrue();
		await Assert.That(withdrawalResult).IsTrue();
		await Assert.That(buyResult).IsFalse();
	}

	[Test]
	public void IsValidForAssetType_UnknownAssetType_ThrowsInvalidOperationException()
	{
		var asset = CreateAsset(AssetType.Unknown);

		Assert.Throws<InvalidOperationException>(
			() => TransactionValidationExtensions.IsValidForAssetType(TransactionType.Deposit, asset));
	}

	private static Asset CreateAsset(AssetType assetType)
	{
		return new Asset(
			new AssetId(Guid.NewGuid()),
			"Test Asset",
			AssetClass.Unknown,
			assetType,
			Currency.INR,
			string.Empty);
	}
}
