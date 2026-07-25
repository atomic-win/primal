using System.Net;

namespace InvestmentPortfolioTracker.E2ETests.AssetItems.GetValuations;

public sealed class Returns_ETF_Invested_From_Buy_Minus_Sell
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new InvestmentPortfolioTrackerE2EFactory();
		_ = factory.CreateClient();

		factory.AlphaVantageApi.SetupEtfSearch(symbol: "CNDX.LON");
		factory.AlphaVantageApi.SetupStockPrices(
			prices: [("2026-05-15", 150.0m), ("2026-05-20", 155.0m), ("2026-06-01", 180.0m), ("2026-05-31", 178.0m)]);

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "iShares NASDAQ 100 ETF",
			assetClass: "Unknown",
			assetType: "Stock",
			externalId: "CNDX.LON",
			currency: "Unknown");

		await client.AddTransactionAsync(
			assetItemId: assetItem.Id,
			date: "2026-05-15",
			name: "Buy ETF",
			transactionType: "Buy",
			units: 10.0m,
			price: 150.0m,
			amount: 0);

		await client.AddTransactionAsync(
			assetItemId: assetItem.Id,
			date: "2026-05-20",
			name: "Sell ETF",
			transactionType: "Sell",
			units: 3.0m,
			price: 155.0m,
			amount: 0);

		// Act
		var response = await client.GetAsync(
			$"/api/asset-items/valuations?currency=USD&assetItemIds={assetItem.Id}");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
