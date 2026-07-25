using System.Net;
using System.Net.Http.Json;

namespace InvestmentPortfolioTracker.E2ETests.Transactions.AddTransaction;

public sealed class Returns_201_When_Deposit_Valid_For_EPF
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
			name: "Test EPF",
			assetClass: "Debt",
			assetType: "EPF",
			externalId: string.Empty,
			currency: "INR");

		// Act
		var response = await client.PostAsJsonAsync(
			$"/api/asset-items/{assetItem.Id}/transactions", new
			{
				AssetItemId = assetItem.Id,
				Date = "2026-01-15",
				Name = "Valid Transaction",
				TransactionType = "Deposit",
				Units = 0,
				Price = 0,
				Amount = 1000,
			});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
