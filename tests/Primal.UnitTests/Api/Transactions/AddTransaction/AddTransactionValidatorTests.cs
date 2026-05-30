using System.Security.Claims;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Primal.Api.Transactions;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.UnitTests.Api.Transactions.AddTransaction;

public sealed class AddTransactionValidatorTests
{
	[Test]
	public async Task ValidateAsync_ReturnsValid_ForStockBuyTransaction()
	{
		var validator = CreateValidator(assetType: AssetType.Stock);
		var request = CreateRequest(transactionType: TransactionType.Buy, units: 10m, price: 25m);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsTrue();
	}

	[Test]
	public async Task ValidateAsync_ReturnsValid_ForBankAccountDepositTransaction()
	{
		var validator = CreateValidator(assetType: AssetType.BankAccount);
		var request = CreateRequest(
			transactionType: TransactionType.Deposit,
			units: 0m,
			price: 0m,
			amount: 1000m,
			name: "Monthly deposit");

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
	public async Task ValidateAsync_ReturnsError_WhenDateIsDefault()
	{
		var validator = CreateValidator();
		var request = CreateRequest(date: DateOnly.MinValue);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Transaction date must be provided.");
	}

	[Test]
	public async Task ValidateAsync_ReturnsError_WhenDateIsInTheFuture()
	{
		var validator = CreateValidator();
		var request = CreateRequest(date: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Transaction date cannot be in the future.");
	}

	[Test]
	public async Task ValidateAsync_ReturnsError_WhenNameIsEmpty()
	{
		var validator = CreateValidator();
		var request = CreateRequest(name: string.Empty);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Transaction name must be provided.");
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
	public async Task ValidateAsync_ReturnsError_WhenTransactionTypeIsUnknown()
	{
		var validator = CreateValidator();
		var request = CreateRequest(transactionType: TransactionType.Unknown);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Transaction type must be provided.");
	}

	[Test]
	public async Task ValidateAsync_ReturnsError_WhenTransactionTypeIsInvalidForAssetType()
	{
		var validator = CreateValidator(assetType: AssetType.BankAccount);
		var request = CreateRequest(transactionType: TransactionType.Buy);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Transaction type 'Buy' is not valid for the asset type.");
	}

	[Test]
	[Arguments(TransactionType.Buy, 0)]
	[Arguments(TransactionType.Sell, -1)]
	public async Task ValidateAsync_ReturnsError_WhenUnitsAreNotGreaterThanZeroForBuyOrSell(
		TransactionType transactionType,
		decimal units)
	{
		var validator = CreateValidator(assetType: AssetType.Stock);
		var request = CreateRequest(transactionType: transactionType, units: units, price: 25m);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Transaction units must be greater than zero.");
	}

	[Test]
	[Arguments(TransactionType.Buy, 0)]
	[Arguments(TransactionType.Sell, -1)]
	public async Task ValidateAsync_ReturnsError_WhenPriceIsNotGreaterThanZeroForBuyOrSell(
		TransactionType transactionType,
		decimal price)
	{
		var validator = CreateValidator(assetType: AssetType.Stock);
		var request = CreateRequest(transactionType: transactionType, units: 10m, price: price);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Transaction price must be greater than zero.");
	}

	[Test]
	[Arguments(TransactionType.Deposit, 0)]
	[Arguments(TransactionType.Interest, -1)]
	public async Task ValidateAsync_ReturnsError_WhenAmountIsNotGreaterThanZeroForNonBuyOrSell(
		TransactionType transactionType,
		decimal amount)
	{
		var validator = CreateValidator(assetType: AssetType.BankAccount);
		var request = CreateRequest(
			transactionType: transactionType,
			units: 0m,
			price: 0m,
			amount: amount);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "Transaction amount must be greater than zero.");
	}

	private static AddTransactionValidator CreateValidator(
		AssetType assetType = AssetType.Stock,
		bool assetItemExists = true)
	{
		var assetItemRepository = Substitute.For<IAssetItemRepository>();
		var assetRepository = Substitute.For<IAssetRepository>();
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

		assetItemRepository.GetByIdAsync(Arg.Any<Domain.Users.UserId>(), Arg.Any<AssetItemId>(), Arg.Any<CancellationToken>())
			.Returns(assetItem);
		assetRepository.GetByIdAsync(Arg.Any<AssetId>(), Arg.Any<CancellationToken>())
			.Returns(asset);

		return new AddTransactionValidator(assetItemRepository, assetRepository, CreateMockHttpContextAccessor());
	}

	private static AddTransactionRequest CreateRequest(
		Guid? assetItemId = null,
		DateOnly? date = null,
		string name = "Buy transaction",
		TransactionType transactionType = TransactionType.Buy,
		decimal units = 10m,
		decimal price = 25m,
		decimal amount = 0m)
	{
		return new AddTransactionRequest(
			assetItemId ?? Guid.NewGuid(),
			date ?? DateOnly.FromDateTime(DateTime.UtcNow),
			name,
			transactionType,
			units,
			price,
			amount);
	}

	private static IHttpContextAccessor CreateMockHttpContextAccessor()
	{
		var userId = Guid.NewGuid();
		var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
		var identity = new ClaimsIdentity(claims);
		var principal = new ClaimsPrincipal(identity);
		var httpContext = new DefaultHttpContext { User = principal };
		var accessor = Substitute.For<IHttpContextAccessor>();
		accessor.HttpContext.Returns(httpContext);
		return accessor;
	}

	private static async Task AssertHasError(ValidationResult result, string errorMessage)
	{
		await Assert.That(result.Errors.Any(x => string.Equals(x.ErrorMessage, errorMessage, StringComparison.Ordinal))).IsTrue();
	}
}
