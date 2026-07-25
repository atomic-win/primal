using System.Net;

namespace InvestmentPortfolioTracker.E2ETests.AssetItems.GetValuations;

public sealed class Returns_Xirr_With_Actual_YearDiff_When_Over_One_Year
{
	private static readonly (string Date, string Nav)[] MonthlyPrices =
	[
		("15-06-2024", "80.00"),
		("30-06-2024", "82.00"),
		("31-07-2024", "84.00"),
		("31-08-2024", "86.00"),
		("30-09-2024", "88.00"),
		("31-10-2024", "90.00"),
		("30-11-2024", "92.00"),
		("31-12-2024", "94.00"),
		("31-01-2025", "96.00"),
		("28-02-2025", "98.00"),
		("31-03-2025", "100.00"),
		("30-04-2025", "102.00"),
		("31-05-2025", "104.00"),
		("30-06-2025", "106.00"),
		("31-07-2025", "108.00"),
		("31-08-2025", "110.00"),
		("30-09-2025", "112.00"),
		("31-10-2025", "114.00"),
		("30-11-2025", "116.00"),
		("31-12-2025", "118.00"),
		("31-01-2026", "120.00"),
		("28-02-2026", "122.00"),
		("31-03-2026", "124.00"),
		("30-04-2026", "126.00"),
		("31-05-2026", "128.00"),
		("01-06-2026", "130.00"),
	];

	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new InvestmentPortfolioTrackerE2EFactory();
		_ = factory.CreateClient();

		factory.MutualFundApi.SetupMutualFundLatest(schemeCode: "119551");
		factory.MutualFundApi.SetupMutualFundPrices(schemeCode: "119551", prices: MonthlyPrices);

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "Test MF",
			assetClass: "Equity",
			assetType: "MutualFund",
			externalId: "119551",
			currency: "Unknown");

		// Buy 2 years ago — YearDiff > 1
		await client.AddTransactionAsync(
			assetItemId: assetItem.Id,
			date: "2024-06-15",
			name: "Buy Units",
			transactionType: "Buy",
			units: 10.0m,
			price: 80.0m,
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
