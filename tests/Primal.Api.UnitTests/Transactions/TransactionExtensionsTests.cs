using NSubstitute;
using Primal.Api.Transactions;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.UnitTests.Transactions;

public sealed class TransactionExtensionsTests
{
	[Test]
	public async Task ToResponse_MapsAllFieldsCorrectly()
	{
		var transactionId = new TransactionId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());
		var userId = new UserId(Guid.NewGuid());
		var date = new DateOnly(2024, 3, 15);

		var transaction = new Transaction(
			transactionId,
			date,
			"Test Transaction",
			TransactionType.Buy,
			assetItemId,
			units: 10m,
			price: 100m,
			amount: 0m);

		var assetItemRepo = Substitute.For<IAssetItemRepository>();
		var assetRepo = Substitute.For<IAssetRepository>();
		var exchangeRateClient = Substitute.For<IExchangeRateApiClient>();
		var mutualFundClient = Substitute.For<IAssetApiClient<MutualFund>>();
		var stockClient = Substitute.For<IAssetApiClient<Stock>>();

		var assetId = new AssetId(Guid.NewGuid());
		var assetItem = new AssetItem(assetItemId, assetId, "Test");
		var asset = new Asset(assetId, "Test Asset", AssetClass.Equity, AssetType.MutualFund, Currency.INR, "mf-12345");

		assetItemRepo.GetByIdAsync(userId, assetItemId, Arg.Any<CancellationToken>()).Returns(assetItem);
		assetRepo.GetByIdAsync(assetId, Arg.Any<CancellationToken>()).Returns(asset);
		exchangeRateClient.GetOnOrBeforeExchangeRateAsync(Currency.INR, Currency.INR, date, Arg.Any<CancellationToken>()).Returns(1m);
		mutualFundClient.GetOnOrBeforePriceAsync("12345", date, Arg.Any<CancellationToken>()).Returns(100m);

		var calculator = new TransactionAmountCalculator(
			mutualFundClient, stockClient, exchangeRateClient, assetItemRepo, assetRepo);

		var response = await transaction.ToResponse(
			userId,
			calculator,
			Currency.INR,
			CancellationToken.None);

		await Assert.That(response.Id).IsEqualTo(transactionId.Value);
		await Assert.That(response.Date).IsEqualTo(date);
		await Assert.That(response.Name).IsEqualTo("Test Transaction");
		await Assert.That(response.TransactionType).IsEqualTo(TransactionType.Buy);
		await Assert.That(response.AssetItemId).IsEqualTo(assetItemId.Value);
		await Assert.That(response.Units).IsEqualTo(10m);
		await Assert.That(response.Price).IsEqualTo(100m);
		await Assert.That(response.Amount).IsEqualTo(1000m);
	}

	[Test]
	public async Task ToResponse_DepositTransaction_ReturnsAmountWithExchangeRate()
	{
		var transactionId = new TransactionId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());
		var userId = new UserId(Guid.NewGuid());
		var transactionDate = new DateOnly(2024, 1, 10);

		var transaction = new Transaction(
			transactionId,
			transactionDate,
			"Deposit",
			TransactionType.Deposit,
			assetItemId,
			units: 0m,
			price: 0m,
			amount: 5000m);

		var assetItemRepo = Substitute.For<IAssetItemRepository>();
		var assetRepo = Substitute.For<IAssetRepository>();
		var exchangeRateClient = Substitute.For<IExchangeRateApiClient>();
		var mutualFundClient = Substitute.For<IAssetApiClient<MutualFund>>();
		var stockClient = Substitute.For<IAssetApiClient<Stock>>();

		var assetId = new AssetId(Guid.NewGuid());
		var assetItem = new AssetItem(assetItemId, assetId, "Bank");
		var asset = new Asset(assetId, "Bank Account", AssetClass.Debt, AssetType.BankAccount, Currency.INR, "default-Debt-BankAccount-INR");

		assetItemRepo.GetByIdAsync(userId, assetItemId, Arg.Any<CancellationToken>()).Returns(assetItem);
		assetRepo.GetByIdAsync(assetId, Arg.Any<CancellationToken>()).Returns(asset);
		exchangeRateClient.GetOnOrBeforeExchangeRateAsync(Currency.INR, Currency.USD, transactionDate, Arg.Any<CancellationToken>()).Returns(0.012m);

		var calculator = new TransactionAmountCalculator(
			mutualFundClient, stockClient, exchangeRateClient, assetItemRepo, assetRepo);

		var response = await transaction.ToResponse(
			userId,
			calculator,
			Currency.USD,
			CancellationToken.None);

		await Assert.That(response.Amount).IsEqualTo(60m);
	}
}
