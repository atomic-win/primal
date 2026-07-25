using System.Net;

namespace InvestmentPortfolioTracker.E2ETests.Transactions.GetTransactionById;

public sealed class Returns_400_When_Currency_Is_Missing
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
			units: 0,
			price: 0,
			amount: 5000);

		// Act — no currency query param
		var response = await client.GetAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{transaction.Id}");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
