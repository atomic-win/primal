using System.Net;

namespace Primal.E2ETests.Transactions.GetAllByAssetItemId;

public sealed class Returns_Transactions_With_Cross_Currency
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		factory.AlphaVantageApi.SetupForexRate(date: "2026-05-28", closeRate: 83.5m);

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "Test Bank",
			assetClass: "EmergencyFund",
			assetType: "BankAccount",
			externalId: string.Empty,
			currency: "INR");

		await client.AddTransactionAsync(
			assetItemId: assetItem.Id,
			date: "2026-05-28",
			name: "Deposit",
			transactionType: "Deposit",
			units: 0,
			price: 0,
			amount: 10000);

		// Act
		var response = await client.GetAsync(
			$"/api/asset-items/{assetItem.Id}/transactions?currency=USD");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
