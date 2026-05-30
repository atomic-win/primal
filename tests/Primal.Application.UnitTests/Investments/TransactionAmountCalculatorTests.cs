using NSubstitute;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Application.UnitTests.Investments;

public sealed class TransactionAmountCalculatorTests
{
	[Test]
	public async Task CalculateAmountAsync_DepositTransaction_ReturnsAmountMultipliedByExchangeRate()
	{
		var userId = new UserId(Guid.NewGuid());
		var date = new DateOnly(2024, 1, 15);
		var assetItem = new AssetItem(new AssetItemId(Guid.NewGuid()), new AssetId(Guid.NewGuid()), "Savings");
		var asset = new Asset(assetItem.AssetId, "Savings Account", AssetClass.Debt, AssetType.BankAccount, Currency.USD, string.Empty);
		var transaction = new Transaction(new TransactionId(Guid.NewGuid()), date, "Deposit", TransactionType.Deposit, assetItem.Id, 0m, 0m, 100m);
		var (sut, mutualFundApiClient, stockApiClient, exchangeRateApiClient, assetItemRepository, assetRepository) = CreateSut();
		SetUpRepositories(assetItemRepository, assetRepository, userId, assetItem, asset);
		exchangeRateApiClient
			.GetOnOrBeforeExchangeRateAsync(Currency.USD, Currency.INR, date, CancellationToken.None)
			.Returns(Task.FromResult(82m));

		var result = await sut.CalculateAmountAsync(userId, transaction, date, Currency.INR, CancellationToken.None);

		await Assert.That(result == 8200m).IsTrue();
		_ = mutualFundApiClient.DidNotReceive().GetOnOrBeforePriceAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
		_ = stockApiClient.DidNotReceive().GetOnOrBeforePriceAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task CalculateAmountAsync_BuyTransactionOnSameDate_ReturnsUnitsTimesPriceMultipliedByExchangeRate()
	{
		var userId = new UserId(Guid.NewGuid());
		var date = new DateOnly(2024, 2, 1);
		var assetItem = new AssetItem(new AssetItemId(Guid.NewGuid()), new AssetId(Guid.NewGuid()), "Brokerage");
		var asset = new Asset(assetItem.AssetId, "Tech Stock", AssetClass.Equity, AssetType.Stock, Currency.USD, "stock-MSFT");
		var transaction = new Transaction(new TransactionId(Guid.NewGuid()), date, "Buy", TransactionType.Buy, assetItem.Id, 3m, 25m, 0m);
		var (sut, mutualFundApiClient, stockApiClient, exchangeRateApiClient, assetItemRepository, assetRepository) = CreateSut();
		SetUpRepositories(assetItemRepository, assetRepository, userId, assetItem, asset);
		exchangeRateApiClient
			.GetOnOrBeforeExchangeRateAsync(Currency.USD, Currency.INR, date, CancellationToken.None)
			.Returns(Task.FromResult(83m));

		var result = await sut.CalculateAmountAsync(userId, transaction, date, Currency.INR, CancellationToken.None);

		await Assert.That(result == 6225m).IsTrue();
		_ = mutualFundApiClient.DidNotReceive().GetOnOrBeforePriceAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
		_ = stockApiClient.DidNotReceive().GetOnOrBeforePriceAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task CalculateAmountAsync_BuyTransactionOnDifferentDateWithMutualFund_ReturnsUnitsTimesMutualFundPriceMultipliedByExchangeRate()
	{
		var userId = new UserId(Guid.NewGuid());
		var transactionDate = new DateOnly(2024, 1, 10);
		var valuationDate = new DateOnly(2024, 1, 20);
		var assetItem = new AssetItem(new AssetItemId(Guid.NewGuid()), new AssetId(Guid.NewGuid()), "Retirement");
		var asset = new Asset(assetItem.AssetId, "Index Fund", AssetClass.Equity, AssetType.MutualFund, Currency.USD, "mf-12345");
		var transaction = new Transaction(new TransactionId(Guid.NewGuid()), transactionDate, "Buy", TransactionType.Buy, assetItem.Id, 10m, 0m, 0m);
		var (sut, mutualFundApiClient, stockApiClient, exchangeRateApiClient, assetItemRepository, assetRepository) = CreateSut();
		SetUpRepositories(assetItemRepository, assetRepository, userId, assetItem, asset);
		exchangeRateApiClient
			.GetOnOrBeforeExchangeRateAsync(Currency.USD, Currency.INR, valuationDate, CancellationToken.None)
			.Returns(Task.FromResult(2m));
		mutualFundApiClient
			.GetOnOrBeforePriceAsync("12345", valuationDate, CancellationToken.None)
			.Returns(Task.FromResult(15m));

		var result = await sut.CalculateAmountAsync(userId, transaction, valuationDate, Currency.INR, CancellationToken.None);

		await Assert.That(result == 300m).IsTrue();
		_ = mutualFundApiClient.Received(1).GetOnOrBeforePriceAsync("12345", valuationDate, CancellationToken.None);
		_ = stockApiClient.DidNotReceive().GetOnOrBeforePriceAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
	}

	[Test]
	[Arguments(TransactionType.Buy)]
	[Arguments(TransactionType.Sell)]
	public async Task CalculateAmountAsync_BuyOrSellTransactionOnDifferentDateWithStock_ReturnsUnitsTimesStockPriceMultipliedByExchangeRate(TransactionType transactionType)
	{
		var userId = new UserId(Guid.NewGuid());
		var transactionDate = new DateOnly(2024, 3, 5);
		var valuationDate = new DateOnly(2024, 3, 8);
		var assetItem = new AssetItem(new AssetItemId(Guid.NewGuid()), new AssetId(Guid.NewGuid()), "Equities");
		var asset = new Asset(assetItem.AssetId, "Apple", AssetClass.Equity, AssetType.Stock, Currency.USD, "stock-AAPL");
		var transaction = new Transaction(new TransactionId(Guid.NewGuid()), transactionDate, transactionType.ToString(), transactionType, assetItem.Id, 3m, 0m, 0m);
		var (sut, mutualFundApiClient, stockApiClient, exchangeRateApiClient, assetItemRepository, assetRepository) = CreateSut();
		SetUpRepositories(assetItemRepository, assetRepository, userId, assetItem, asset);
		exchangeRateApiClient
			.GetOnOrBeforeExchangeRateAsync(Currency.USD, Currency.INR, valuationDate, CancellationToken.None)
			.Returns(Task.FromResult(1.5m));
		stockApiClient
			.GetOnOrBeforePriceAsync("AAPL", valuationDate, CancellationToken.None)
			.Returns(Task.FromResult(12m));

		var result = await sut.CalculateAmountAsync(userId, transaction, valuationDate, Currency.INR, CancellationToken.None);

		await Assert.That(result == 54m).IsTrue();
		_ = stockApiClient.Received(1).GetOnOrBeforePriceAsync("AAPL", valuationDate, CancellationToken.None);
		_ = mutualFundApiClient.DidNotReceive().GetOnOrBeforePriceAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task CalculateAmountAsync_BuyTransactionOnDifferentDateWithBankAccount_ReturnsUnitsMultipliedByExchangeRate()
	{
		var userId = new UserId(Guid.NewGuid());
		var transactionDate = new DateOnly(2024, 4, 1);
		var valuationDate = new DateOnly(2024, 4, 10);
		var assetItem = new AssetItem(new AssetItemId(Guid.NewGuid()), new AssetId(Guid.NewGuid()), "Cash");
		var asset = new Asset(assetItem.AssetId, "Bank Account", AssetClass.Debt, AssetType.BankAccount, Currency.USD, string.Empty);
		var transaction = new Transaction(new TransactionId(Guid.NewGuid()), transactionDate, "Buy", TransactionType.Buy, assetItem.Id, 10m, 0m, 0m);
		var (sut, mutualFundApiClient, stockApiClient, exchangeRateApiClient, assetItemRepository, assetRepository) = CreateSut();
		SetUpRepositories(assetItemRepository, assetRepository, userId, assetItem, asset);
		exchangeRateApiClient
			.GetOnOrBeforeExchangeRateAsync(Currency.USD, Currency.INR, valuationDate, CancellationToken.None)
			.Returns(Task.FromResult(2m));

		var result = await sut.CalculateAmountAsync(userId, transaction, valuationDate, Currency.INR, CancellationToken.None);

		await Assert.That(result == 20m).IsTrue();
		_ = mutualFundApiClient.DidNotReceive().GetOnOrBeforePriceAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
		_ = stockApiClient.DidNotReceive().GetOnOrBeforePriceAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task CalculateAmountAsync_SameCurrency_ReturnsUnchangedAmount()
	{
		var userId = new UserId(Guid.NewGuid());
		var date = new DateOnly(2024, 5, 15);
		var assetItem = new AssetItem(new AssetItemId(Guid.NewGuid()), new AssetId(Guid.NewGuid()), "Wallet");
		var asset = new Asset(assetItem.AssetId, "Wallet", AssetClass.EmergencyFund, AssetType.Wallet, Currency.INR, string.Empty);
		var transaction = new Transaction(new TransactionId(Guid.NewGuid()), date, "Deposit", TransactionType.Deposit, assetItem.Id, 0m, 0m, 123.45m);
		var (sut, _, _, exchangeRateApiClient, assetItemRepository, assetRepository) = CreateSut();
		SetUpRepositories(assetItemRepository, assetRepository, userId, assetItem, asset);
		exchangeRateApiClient
			.GetOnOrBeforeExchangeRateAsync(Currency.INR, Currency.INR, date, CancellationToken.None)
			.Returns(Task.FromResult(1m));

		var result = await sut.CalculateAmountAsync(userId, transaction, date, Currency.INR, CancellationToken.None);

		await Assert.That(result == 123.45m).IsTrue();
	}

	private static (TransactionAmountCalculator Sut, IAssetApiClient<MutualFund> MutualFundApiClient, IAssetApiClient<Stock> StockApiClient, IExchangeRateApiClient ExchangeRateApiClient, IAssetItemRepository AssetItemRepository, IAssetRepository AssetRepository) CreateSut()
	{
		var mutualFundApiClient = Substitute.For<IAssetApiClient<MutualFund>>();
		var stockApiClient = Substitute.For<IAssetApiClient<Stock>>();
		var exchangeRateApiClient = Substitute.For<IExchangeRateApiClient>();
		var assetItemRepository = Substitute.For<IAssetItemRepository>();
		var assetRepository = Substitute.For<IAssetRepository>();
		var sut = new TransactionAmountCalculator(
			mutualFundApiClient,
			stockApiClient,
			exchangeRateApiClient,
			assetItemRepository,
			assetRepository);

		return (sut, mutualFundApiClient, stockApiClient, exchangeRateApiClient, assetItemRepository, assetRepository);
	}

	private static void SetUpRepositories(
		IAssetItemRepository assetItemRepository,
		IAssetRepository assetRepository,
		UserId userId,
		AssetItem assetItem,
		Asset asset)
	{
		assetItemRepository
			.GetByIdAsync(userId, assetItem.Id, CancellationToken.None)
			.Returns(Task.FromResult(assetItem));
		assetRepository
			.GetByIdAsync(asset.Id, CancellationToken.None)
			.Returns(Task.FromResult(asset));
	}
}
