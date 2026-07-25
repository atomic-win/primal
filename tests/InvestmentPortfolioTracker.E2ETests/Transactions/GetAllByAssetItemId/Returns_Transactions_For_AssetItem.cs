using System.Net;

namespace InvestmentPortfolioTracker.E2ETests.Transactions.GetAllByAssetItemId;

public sealed class Returns_Transactions_For_AssetItem
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new InvestmentPortfolioTrackerE2EFactory();
		_ = factory.CreateClient();

		factory.MutualFundApi.SetupMutualFundLatest(schemeCode: "119551");
		factory.MutualFundApi.SetupMutualFundPrices(
			schemeCode: "119551",
			prices: [("15-01-2026", "150.25"), ("16-01-2026", "151.00")]);

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "Test Mutual Fund",
			assetClass: "Equity",
			assetType: "MutualFund",
			externalId: "119551",
			currency: "Unknown");

		await client.AddTransactionAsync(
			assetItemId: assetItem.Id,
			date: "2026-01-15",
			name: "Buy Units Batch 1",
			transactionType: "Buy",
			units: 10.0m,
			price: 150.25m,
			amount: 0);

		await client.AddTransactionAsync(
			assetItemId: assetItem.Id,
			date: "2026-01-16",
			name: "Buy Units Batch 2",
			transactionType: "Buy",
			units: 5.0m,
			price: 151.00m,
			amount: 0);

		// Act
		var response = await client.GetAsync(
			$"/api/asset-items/{assetItem.Id}/transactions?currency=INR");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
