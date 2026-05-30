using FluentValidation.Results;
using Primal.Api.AssetItems;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.UnitTests.Api.AssetItems.AddAssetItem;

public sealed class AddAssetItemValidatorTests
{
	[Test]
	public async Task ValidateAsync_ReturnsValid_ForMutualFundWithSupportedAssetClass()
	{
		var validator = new AddAssetItemValidator();
		var request = new AddAssetItemRequest(
			"Index Fund",
			AssetClass.Equity,
			AssetType.MutualFund,
			"MF-123",
			Currency.Unknown);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsTrue();
	}

	[Test]
	public async Task ValidateAsync_ReturnsValid_ForBankAccountWithCurrencyAndAssetClass()
	{
		var validator = new AddAssetItemValidator();
		var request = new AddAssetItemRequest(
			"Emergency Fund",
			AssetClass.EmergencyFund,
			AssetType.BankAccount,
			string.Empty,
			Currency.USD);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsTrue();
	}

	[Test]
	public async Task ValidateAsync_ReturnsError_WhenAssetTypeIsUnknown()
	{
		var validator = new AddAssetItemValidator();
		var request = new AddAssetItemRequest(
			"Cash",
			AssetClass.EmergencyFund,
			AssetType.Unknown,
			string.Empty,
			Currency.USD);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Asset type cannot be Unknown");
	}

	[Test]
	public async Task ValidateAsync_ReturnsError_WhenNameIsEmpty()
	{
		var validator = new AddAssetItemValidator();
		var request = new AddAssetItemRequest(
			string.Empty,
			AssetClass.EmergencyFund,
			AssetType.BankAccount,
			string.Empty,
			Currency.USD);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Name cannot be empty");
	}

	[Test]
	[Arguments(AssetType.BankAccount)]
	[Arguments(AssetType.Wallet)]
	public async Task ValidateAsync_ReturnsError_WhenAssetClassIsMissingForNonStockOrBond(AssetType assetType)
	{
		var validator = new AddAssetItemValidator();
		var request = new AddAssetItemRequest(
			"Cash",
			AssetClass.Unknown,
			assetType,
			string.Empty,
			Currency.USD);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, $"Asset class must be specified for {assetType} asset type");
	}

	[Test]
	[Arguments(AssetType.Stock)]
	[Arguments(AssetType.Bond)]
	public async Task ValidateAsync_ReturnsError_WhenAssetClassIsSpecifiedForStockOrBond(AssetType assetType)
	{
		var validator = new AddAssetItemValidator();
		var request = new AddAssetItemRequest(
			"Holding",
			AssetClass.Equity,
			assetType,
			assetType == AssetType.Stock ? "STK-123" : string.Empty,
			Currency.Unknown);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, $"Asset class must not be specified for {assetType} asset type");
	}

	[Test]
	public async Task ValidateAsync_ReturnsError_WhenMutualFundHasUnsupportedAssetClass()
	{
		var validator = new AddAssetItemValidator();
		var request = new AddAssetItemRequest(
			"Fund",
			AssetClass.EmergencyFund,
			AssetType.MutualFund,
			"MF-123",
			Currency.Unknown);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Asset class 'EmergencyFund' is not valid for MutualFund asset type");
	}

	[Test]
	[Arguments(AssetType.MutualFund)]
	[Arguments(AssetType.Stock)]
	public async Task ValidateAsync_ReturnsError_WhenExternalIdIsMissingForMutualFundOrStock(AssetType assetType)
	{
		var validator = new AddAssetItemValidator();
		var request = new AddAssetItemRequest(
			"Asset",
			assetType == AssetType.MutualFund ? AssetClass.Equity : AssetClass.Unknown,
			assetType,
			string.Empty,
			Currency.Unknown);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, $"ExternalId must be specified for {assetType} asset type");
	}

	[Test]
	[Arguments(AssetType.BankAccount)]
	[Arguments(AssetType.Bond)]
	public async Task ValidateAsync_ReturnsError_WhenExternalIdIsSpecifiedForOtherAssetTypes(AssetType assetType)
	{
		var validator = new AddAssetItemValidator();
		var request = new AddAssetItemRequest(
			"Asset",
			assetType == AssetType.Bond ? AssetClass.Unknown : AssetClass.EmergencyFund,
			assetType,
			"EXT-123",
			Currency.USD);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, $"ExternalId must not be specified for {assetType} asset type");
	}

	[Test]
	[Arguments(AssetType.BankAccount)]
	[Arguments(AssetType.Bond)]
	public async Task ValidateAsync_ReturnsError_WhenCurrencyIsMissingForNonMutualFundOrStock(AssetType assetType)
	{
		var validator = new AddAssetItemValidator();
		var request = new AddAssetItemRequest(
			"Asset",
			assetType == AssetType.Bond ? AssetClass.Unknown : AssetClass.EmergencyFund,
			assetType,
			string.Empty,
			Currency.Unknown);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, $"Currency must be specified for {assetType} asset type");
	}

	[Test]
	[Arguments(AssetType.MutualFund)]
	[Arguments(AssetType.Stock)]
	public async Task ValidateAsync_ReturnsError_WhenCurrencyIsSpecifiedForMutualFundOrStock(AssetType assetType)
	{
		var validator = new AddAssetItemValidator();
		var request = new AddAssetItemRequest(
			"Asset",
			assetType == AssetType.MutualFund ? AssetClass.Equity : AssetClass.Unknown,
			assetType,
			"EXT-123",
			Currency.INR);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, $"Currency must not be specified for {assetType} asset type");
	}

	private static async Task AssertHasError(ValidationResult result, string errorMessage)
	{
		await Assert.That(result.Errors.Any(x => string.Equals(x.ErrorMessage, errorMessage, StringComparison.Ordinal))).IsTrue();
	}
}
