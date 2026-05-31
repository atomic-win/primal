using FluentValidation.Results;
using NSubstitute;
using Primal.Api.Transactions;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Api.UnitTests.Api.Transactions.EditTransaction;

public sealed class EditTransactionValidatorTests
{
	private static readonly Guid TestUserId = Guid.NewGuid();

	[Test]
	public async Task ValidateAsync_ReturnsValid_ForPartialUpdateWithoutOptionalFields()
	{
		var validator = CreateValidator(assetType: AssetType.Stock);
		var request = CreateRequest();

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsTrue();
	}

	[Test]
	public async Task ValidateAsync_ReturnsValid_WhenBuyTransactionUsesZeroUnitsAndPrice()
	{
		var validator = CreateValidator(assetType: AssetType.Stock);
		var request = CreateRequest(
			name: "Update buy",
			transactionType: TransactionType.Buy,
			units: 0m,
			price: 0m);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsTrue();
	}

	[Test]
	public async Task ValidateAsync_ReturnsValid_WhenDepositUsesZeroAmount()
	{
		var validator = CreateValidator(assetType: AssetType.BankAccount);
		var request = CreateRequest(
			name: "Update deposit",
			transactionType: TransactionType.Deposit,
			amount: 0m);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsTrue();
	}

	[Test]
	public async Task ValidateAsync_ReturnsError_WhenAssetItemIdIsEmpty()
	{
		var validator = CreateValidator();
		var request = CreateRequest(assetItemId: Guid.Empty);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Asset item ID must be provided.");
	}

	[Test]
	public async Task ValidateAsync_ReturnsError_WhenAssetItemDoesNotExist()
	{
		var validator = CreateValidator(assetItemExists: false);
		var request = CreateRequest();

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Asset item does not exist.");
	}

	[Test]
	public async Task ValidateAsync_ReturnsError_WhenTransactionDoesNotExist()
	{
		var validator = CreateValidator(transactionExists: false);
		var request = CreateRequest();

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Transaction does not exist.");
	}

	[Test]
	public async Task ValidateAsync_ReturnsError_WhenNameIsTooShort()
	{
		var validator = CreateValidator();
		var request = CreateRequest(name: "AB");

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Transaction name must be at least 3 characters long.");
	}

	[Test]
	public async Task ValidateAsync_ReturnsError_WhenNameIsTooLong()
	{
		var validator = CreateValidator();
		var request = CreateRequest(name: new string('A', 1001));

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Transaction name must not exceed 1000 characters.");
	}

	[Test]
	public async Task ValidateAsync_ReturnsError_WhenTransactionTypeIsInvalidForAssetType()
	{
		var validator = CreateValidator(assetType: AssetType.BankAccount);
		var request = CreateRequest(name: "Invalid type", transactionType: TransactionType.Buy);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Transaction type 'Buy' is not valid for the asset type.");
	}

	[Test]
	[Arguments(TransactionType.Buy, -1)]
	[Arguments(TransactionType.Sell, -5)]
	public async Task ValidateAsync_ReturnsError_WhenUnitsAreNegativeForBuyOrSell(
		TransactionType transactionType,
		decimal units)
	{
		var validator = CreateValidator(assetType: AssetType.Stock);
		var request = CreateRequest(
			name: "Negative units",
			transactionType: transactionType,
			units: units,
			price: 10m);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Transaction units must be greater than or equal to zero.");
	}

	[Test]
	[Arguments(TransactionType.Buy, -1)]
	[Arguments(TransactionType.Sell, -5)]
	public async Task ValidateAsync_ReturnsError_WhenPriceIsNegativeForBuyOrSell(
		TransactionType transactionType,
		decimal price)
	{
		var validator = CreateValidator(assetType: AssetType.Stock);
		var request = CreateRequest(
			name: "Negative price",
			transactionType: transactionType,
			units: 10m,
			price: price);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Transaction price must be greater than or equal to zero.");
	}

	[Test]
	[Arguments(TransactionType.Deposit, -1)]
	[Arguments(TransactionType.Interest, -5)]
	public async Task ValidateAsync_ReturnsError_WhenAmountIsNegativeForNonBuyOrSell(
		TransactionType transactionType,
		decimal amount)
	{
		var validator = CreateValidator(assetType: AssetType.BankAccount);
		var request = CreateRequest(
			name: "Negative amount",
			transactionType: transactionType,
			amount: amount);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Transaction amount must be greater than or equal to zero.");
	}

	private static EditTransactionValidator CreateValidator(
		AssetType assetType = AssetType.Stock,
		bool assetItemExists = true,
		bool transactionExists = true)
	{
		var assetItemRepository = Substitute.For<IAssetItemRepository>();
		var assetRepository = Substitute.For<IAssetRepository>();
		var transactionRepository = Substitute.For<ITransactionRepository>();
		var assetItem = assetItemExists
			? new AssetItem(new AssetItemId(Guid.NewGuid()), new AssetId(Guid.NewGuid()), "Asset Item")
			: AssetItem.Empty;
		var asset = new Asset(
			assetItem.AssetId,
			"Asset",
			assetType == AssetType.Stock ? AssetClass.Unknown : AssetClass.EmergencyFund,
			assetType,
			assetType is AssetType.Stock or AssetType.MutualFund ? Currency.Unknown : Currency.USD,
			assetType is AssetType.Stock or AssetType.MutualFund ? "EXT-123" : string.Empty);
		var transaction = transactionExists
			? new Transaction(
				new TransactionId(Guid.NewGuid()),
				DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
				"Existing transaction",
				TransactionType.Deposit,
				assetItem.Id,
				0m,
				0m,
				100m)
			: Transaction.Empty;

		assetItemRepository.GetByIdAsync(Arg.Any<Domain.Users.UserId>(), Arg.Any<AssetItemId>(), Arg.Any<CancellationToken>())
			.Returns(assetItem);
		assetRepository.GetByIdAsync(Arg.Any<AssetId>(), Arg.Any<CancellationToken>())
			.Returns(asset);
		transactionRepository.GetByIdAsync(Arg.Any<Domain.Users.UserId>(), Arg.Any<AssetItemId>(), Arg.Any<TransactionId>(), Arg.Any<CancellationToken>())
			.Returns(transaction);

		return new EditTransactionValidator(
			assetItemRepository,
			assetRepository,
			transactionRepository);
	}

	private static EditTransactionRequest CreateRequest(
		Guid? assetItemId = null,
		Guid? transactionId = null,
		string name = "",
		TransactionType transactionType = TransactionType.Unknown,
		decimal units = 0m,
		decimal price = 0m,
		decimal amount = 0m)
	{
		return new EditTransactionRequest(
			TestUserId,
			assetItemId ?? Guid.NewGuid(),
			transactionId ?? Guid.NewGuid(),
			name,
			transactionType,
			units,
			price,
			amount);
	}

	private static async Task AssertHasError(ValidationResult result, string errorMessage)
	{
		await Assert.That(result.Errors.Any(x => string.Equals(x.ErrorMessage, errorMessage, StringComparison.Ordinal))).IsTrue();
	}
}
