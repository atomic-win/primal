using System.Net;

namespace InvestmentPortfolioTracker.E2ETests.AssetItems.GetValuations;

public sealed class Returns_Valuations_For_Valid_Request
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new InvestmentPortfolioTrackerE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		// Create a BankAccount asset item (no external API needed)
		var assetItem = await client.AddAssetItemAsync(
			"My Bank", "EmergencyFund", "BankAccount", string.Empty, "INR");

		// Act
		var response = await client.GetAsync(
			$"/api/asset-items/valuations?currency=INR&assetItemIds={assetItem.Id}");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
