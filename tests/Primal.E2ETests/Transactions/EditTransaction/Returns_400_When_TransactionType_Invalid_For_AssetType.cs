using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.Transactions.EditTransaction;

public sealed class Returns_400_When_TransactionType_Invalid_For_AssetType
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
			name: "Deposit",
			transactionType: "Deposit",
			units: 0m,
			price: 0m,
			amount: 100m);

		// Act
		var response = await client.PatchAsJsonAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{transaction.Id}",
			new
			{
				AssetItemId = assetItem.Id,
				TransactionId = transaction.Id,
				Name = "Changed to Buy",
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
