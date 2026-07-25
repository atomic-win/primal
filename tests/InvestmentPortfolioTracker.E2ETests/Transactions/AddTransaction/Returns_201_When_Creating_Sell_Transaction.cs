using System.Net;
using System.Net.Http.Json;

namespace InvestmentPortfolioTracker.E2ETests.Transactions.AddTransaction;

public sealed class Returns_201_When_Creating_Sell_Transaction
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

		// Act
		var response = await client.PostAsJsonAsync(
			$"/api/asset-items/{assetItem.Id}/transactions", new
			{
				AssetItemId = assetItem.Id,
				Date = "2026-01-15",
				Name = "Sell Units",
				TransactionType = "Sell",
				Units = 5.0,
				Price = 120.0,
				Amount = 0,
			});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
