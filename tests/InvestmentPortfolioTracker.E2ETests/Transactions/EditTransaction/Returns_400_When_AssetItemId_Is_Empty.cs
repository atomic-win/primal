using System.Net;
using System.Net.Http.Json;

namespace InvestmentPortfolioTracker.E2ETests.Transactions.EditTransaction;

public sealed class Returns_400_When_AssetItemId_Is_Empty
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new InvestmentPortfolioTrackerE2EFactory();
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
			$"/api/asset-items/{Guid.Empty}/transactions/{transaction.Id}",
			new
			{
				AssetItemId = Guid.Empty,
				TransactionId = transaction.Id,
				Name = "Updated",
				TransactionType = "Unknown",
				Units = 0,
				Price = 0,
				Amount = 200,
			});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
