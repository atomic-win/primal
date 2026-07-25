using System.Net;
using System.Net.Http.Json;

namespace InvestmentPortfolioTracker.E2ETests.AssetItems.EditAssetItem;

public sealed class Returns_204_And_GET_Returns_Updated_Name
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new InvestmentPortfolioTrackerE2EFactory();
		_ = factory.CreateClient();

		factory.MutualFundApi.SetupMutualFundLatest(schemeCode: "119551");

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "Test Mutual Fund",
			assetClass: "Equity",
			assetType: "MutualFund",
			externalId: "119551",
			currency: "Unknown");

		// Act
		var editResponse = await client.PatchAsJsonAsync(
			$"/api/asset-items/{assetItem.Id}",
			new { Name = "Updated Fund Name" });

		// Assert
		await Assert.That(editResponse.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

		var getResponse = await client.GetAsync($"/api/asset-items/{assetItem.Id}");
		var body = await getResponse.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
