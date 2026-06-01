using System.Net;

namespace Primal.E2ETests.Transactions.GetTransactionById;

public sealed class Returns_Deposit_Amount_Multiplied_By_Exchange_Rate
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		factory.ExchangeRateApi.SetupExchangeRate(date: "2026-01-15", closeRate: 82m);

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "USD Bank",
			assetClass: "EmergencyFund",
			assetType: "BankAccount",
			externalId: string.Empty,
			currency: "USD");

		var transaction = await client.AddTransactionAsync(
			assetItemId: assetItem.Id,
			date: "2026-01-15",
			name: "Deposit",
			transactionType: "Deposit",
			units: 0m,
			price: 0m,
			amount: 100m);

		// Act — request in INR (asset is USD, exchange rate = 82)
		var response = await client.GetAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{transaction.Id}?currency=INR");

		// Assert — 100 USD * 82 = 8200 INR
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
