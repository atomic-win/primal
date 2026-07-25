using System.Net;

namespace InvestmentPortfolioTracker.E2ETests.AssetItems.GetValuations;

public sealed class Returns_Buy_Amount_At_Transaction_Date_Price
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
			prices: [("15-05-2026", "100.00"), ("01-06-2026", "150.00"), ("31-05-2026", "148.00")]);

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "Test MF",
			assetClass: "Equity",
			assetType: "MutualFund",
			externalId: "119551",
			currency: "Unknown");

		// Buy at 100, current price is 150 — InvestedValue should use transaction date price
		await client.AddTransactionAsync(
			assetItemId: assetItem.Id,
			date: "2026-05-15",
			name: "Buy Units",
			transactionType: "Buy",
			units: 10.0m,
			price: 100.0m,
			amount: 0);

		// Act
		var response = await client.GetAsync(
			$"/api/asset-items/valuations?currency=INR&assetItemIds={assetItem.Id}");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
