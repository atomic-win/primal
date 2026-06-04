using System.Net;

namespace Primal.E2ETests.Transactions.GetTransactionById;

public sealed class Returns_Buy_Units_Times_Price_Multiplied_By_Exchange_Rate
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		factory.AlphaVantageApi.SetupStockSearch(symbol: "AAPL");
		factory.AlphaVantageApi.SetupForexRate(date: "2026-05-28", closeRate: 83m);

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "Apple Stock",
			assetClass: "Unknown",
			assetType: "Stock",
			externalId: "AAPL",
			currency: "Unknown");

		var transaction = await client.AddTransactionAsync(
			assetItemId: assetItem.Id,
			date: "2026-05-28",
			name: "Buy AAPL",
			transactionType: "Buy",
			units: 3m,
			price: 25m,
			amount: 0m);

		// Act
		var response = await client.GetAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{transaction.Id}?currency=INR");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
