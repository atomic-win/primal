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

		factory.StockApi.SetupStockSearch(symbol: "AAPL");
		factory.ExchangeRateApi.SetupExchangeRate(date: "2026-01-15", closeRate: 83m);

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
			date: "2026-01-15",
			name: "Buy AAPL",
			transactionType: "Buy",
			units: 3m,
			price: 25m,
			amount: 0m);

		// Act — request in INR (stock is USD, exchange rate = 83)
		var response = await client.GetAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{transaction.Id}?currency=INR");

		// Assert — 3 units * 25 price * 83 exchange rate = 6225 INR
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
