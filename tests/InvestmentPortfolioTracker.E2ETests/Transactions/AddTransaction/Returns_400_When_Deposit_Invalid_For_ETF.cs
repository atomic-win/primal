using System.Net;
using System.Net.Http.Json;

namespace InvestmentPortfolioTracker.E2ETests.Transactions.AddTransaction;

public sealed class Returns_400_When_Deposit_Invalid_For_ETF
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new InvestmentPortfolioTrackerE2EFactory();
		_ = factory.CreateClient();

		factory.AlphaVantageApi.SetupEtfSearch(symbol: "CNDX.LON");

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "Test ETF",
			assetClass: "Unknown",
			assetType: "Stock",
			externalId: "CNDX.LON",
			currency: "Unknown");

		// Act
		var response = await client.PostAsJsonAsync(
			$"/api/asset-items/{assetItem.Id}/transactions", new
			{
				AssetItemId = assetItem.Id,
				Date = "2026-01-15",
				Name = "Invalid Deposit",
				TransactionType = "Deposit",
				Units = 0,
				Price = 0,
				Amount = 100,
			});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
