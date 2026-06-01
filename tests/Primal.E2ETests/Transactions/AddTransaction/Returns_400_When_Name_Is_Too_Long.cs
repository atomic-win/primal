using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.Transactions.AddTransaction;

public sealed class Returns_400_When_Name_Is_Too_Long
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

		// Act
		var response = await client.PostAsJsonAsync(
			$"/api/asset-items/{assetItem.Id}/transactions", new
			{
				AssetItemId = assetItem.Id,
				Date = "2026-01-15",
				Name = new string('A', 1001),
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
