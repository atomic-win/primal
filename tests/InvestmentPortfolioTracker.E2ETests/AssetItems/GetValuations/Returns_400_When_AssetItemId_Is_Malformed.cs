using System.Net;

namespace InvestmentPortfolioTracker.E2ETests.AssetItems.GetValuations;

public sealed class Returns_400_When_AssetItemId_Is_Malformed
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new InvestmentPortfolioTrackerE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		// Act
		var response = await client.GetAsync(
			"/api/asset-items/valuations?currency=INR&assetItemIds=not-a-guid");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
