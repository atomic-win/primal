using System.Net;

namespace Primal.E2ETests.Transactions.GetTransactionById;

public sealed class Returns_Units_Multiplied_By_Exchange_Rate_For_BankAccount
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		factory.ExchangeRateApi.SetupExchangeRate(date: "2026-01-15", closeRate: 2m);

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "USD Savings",
			assetClass: "Debt",
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
			amount: 50m);

		// Act
		var response = await client.GetAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{transaction.Id}?currency=INR");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
