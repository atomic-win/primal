using System.Net;

namespace Primal.E2ETests.AssetItems.GetValuations;

public sealed class Returns_TradingAccount_Invested_Equals_Current
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "Test Trading",
			assetClass: "EmergencyFund",
			assetType: "TradingAccount",
			externalId: string.Empty,
			currency: "INR");

		await client.AddTransactionAsync(
			assetItemId: assetItem.Id,
			date: "2026-05-15",
			name: "Deposit",
			transactionType: "Deposit",
			units: 0,
			price: 0,
			amount: 10000);

		// Act
		var response = await client.GetAsync(
			$"/api/asset-items/valuations?currency=INR&assetItemIds={assetItem.Id}");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
