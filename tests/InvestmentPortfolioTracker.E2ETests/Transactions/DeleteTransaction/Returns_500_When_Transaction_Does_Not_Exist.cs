using System.Net;

namespace InvestmentPortfolioTracker.E2ETests.Transactions.DeleteTransaction;

public sealed class Returns_500_When_Transaction_Does_Not_Exist
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
			name: "Test Fixed Deposit",
			assetClass: "Debt",
			assetType: "FixedDeposit",
			externalId: string.Empty,
			currency: "INR");

		// Act
		var response = await client.DeleteAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{Guid.NewGuid()}");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
