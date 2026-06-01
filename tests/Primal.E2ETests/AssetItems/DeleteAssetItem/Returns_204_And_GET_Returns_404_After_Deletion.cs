using System.Net;

namespace Primal.E2ETests.AssetItems.DeleteAssetItem;

public sealed class Returns_204_And_GET_Returns_404_After_Deletion
{
	[Test]
	public async Task Test()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		WireMockSetup.SetupMutualFundLatest(factory.MutualFundApi);

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);
		var assetItemId = await TestDataSeeder.SeedAssetItemViaMutualFundAsync(client);

		var response = await client.DeleteAsync($"/api/asset-items/{assetItemId}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

		// Validate via GET
		var getResponse = await client.GetAsync($"/api/asset-items/{assetItemId}");
		await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}
}
