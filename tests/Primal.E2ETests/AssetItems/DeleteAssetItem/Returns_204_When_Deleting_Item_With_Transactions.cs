using System.Net;

namespace Primal.E2ETests.AssetItems.DeleteAssetItem;

public sealed class Returns_204_When_Deleting_Item_With_Transactions
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
			name: "Test Bank",
			assetClass: "EmergencyFund",
			assetType: "BankAccount",
			externalId: string.Empty,
			currency: "INR");

		await client.AddTransactionAsync(
			assetItemId: assetItem.Id,
			date: "2026-01-15",
			name: "Deposit",
			transactionType: "Deposit",
			units: 0,
			price: 0,
			amount: 5000);

		// Act
		var response = await client.DeleteAsync($"/api/asset-items/{assetItem.Id}");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

		var getResponse = await client.GetAsync($"/api/asset-items/{assetItem.Id}");
		await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);

		var body = await getResponse.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
