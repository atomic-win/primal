using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.Transactions.AddTransaction;

public sealed class Returns_400_When_Buy_Invalid_For_Wallet
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
			name: "Cash Wallet",
			assetClass: "EmergencyFund",
			assetType: "Wallet",
			externalId: string.Empty,
			currency: "INR");

		// Act
		var response = await client.PostAsJsonAsync(
			$"/api/asset-items/{assetItem.Id}/transactions", new
			{
				AssetItemId = assetItem.Id,
				Date = "2026-01-15",
				Name = "Invalid Buy",
				TransactionType = "Buy",
				Units = 10,
				Price = 25,
				Amount = 0,
			});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
