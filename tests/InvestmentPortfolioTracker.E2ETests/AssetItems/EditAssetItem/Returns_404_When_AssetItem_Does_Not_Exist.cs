using System.Net;
using System.Net.Http.Json;

namespace InvestmentPortfolioTracker.E2ETests.AssetItems.EditAssetItem;

public sealed class Returns_404_When_AssetItem_Does_Not_Exist
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
		var response = await client.PatchAsJsonAsync(
			$"/api/asset-items/{Guid.NewGuid()}",
			new { Name = "Updated Name" });

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
