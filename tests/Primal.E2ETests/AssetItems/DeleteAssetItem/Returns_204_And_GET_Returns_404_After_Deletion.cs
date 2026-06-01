using System.Net;

namespace Primal.E2ETests.AssetItems.DeleteAssetItem;

public sealed class Returns_204_And_GET_Returns_404_After_Deletion
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
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
		var response = await client.DeleteAsync($"/api/asset-items/{assetItem.Id}");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

		var getResponse = await client.GetAsync($"/api/asset-items/{assetItem.Id}");
		await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);

		var body = await getResponse.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
