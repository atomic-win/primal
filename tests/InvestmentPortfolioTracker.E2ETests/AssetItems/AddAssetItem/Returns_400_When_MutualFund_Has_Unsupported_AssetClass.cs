using System.Net;
using System.Net.Http.Json;

namespace InvestmentPortfolioTracker.E2ETests.AssetItems.AddAssetItem;

public sealed class Returns_400_When_MutualFund_Has_Unsupported_AssetClass
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
		var response = await client.PostAsJsonAsync("/api/asset-items", new
		{
			Name = "My Fund",
			AssetClass = "EmergencyFund",
			AssetType = "MutualFund",
			ExternalId = "119551",
			Currency = "Unknown",
		});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
