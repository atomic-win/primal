using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.Transactions.EditTransaction;

public sealed class Returns_204_When_Zero_Amount_Preserves_Existing
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

		var transaction = await client.AddTransactionAsync(
			assetItemId: assetItem.Id,
			date: "2026-01-15",
			name: "Original Name",
			transactionType: "Deposit",
			units: 0,
			price: 0,
			amount: 5000);

		// Act
		var editResponse = await client.PatchAsJsonAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{transaction.Id}",
			new
			{
				AssetItemId = assetItem.Id,
				TransactionId = transaction.Id,
				Name = "Updated Name",
				TransactionType = "Deposit",
				Units = 0,
				Price = 0,
				Amount = 0,
			});

		// Assert
		await Assert.That(editResponse.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

		var getResponse = await client.GetAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{transaction.Id}?currency=INR");
		var body = await getResponse.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
