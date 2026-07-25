using System.Net;
using System.Net.Http.Json;

namespace InvestmentPortfolioTracker.E2ETests.Transactions.AddTransaction;

public sealed class Returns_400_When_Buy_Invalid_For_FixedDeposit
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
			name: "Test FD",
			assetClass: "Debt",
			assetType: "FixedDeposit",
			externalId: string.Empty,
			currency: "INR");

		// Act
		var response = await client.PostAsJsonAsync(
			$"/api/asset-items/{assetItem.Id}/transactions", new
			{
				AssetItemId = assetItem.Id,
				Date = "2026-01-15",
				Name = "Invalid Transaction",
				TransactionType = "Buy",
				Units = 10,
				Price = 25,
				Amount = 100,
			});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
