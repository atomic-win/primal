using System.Net;

namespace Primal.E2ETests.AssetItems.DeleteAssetItem;

public sealed class DeleteAssetItem_Tests
{
	[Test]
	public async Task Returns_204_And_GET_Returns_404_After_Deletion()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		WireMockSetup.SetupMutualFundLatest(factory.MutualFundApi);

		var userId = await TestDataSeeder.SeedUserAsync(factory);
		var client = factory.CreateAuthenticatedClient(userId);
		var assetItemId = await TestDataSeeder.SeedAssetItemViaMutualFundAsync(client);

		var response = await client.DeleteAsync($"/api/asset-items/{assetItemId}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

		// Validate via GET
		var getResponse = await client.GetAsync($"/api/asset-items/{assetItemId}");
		await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}

	[Test]
	public async Task Returns_404_When_AssetItem_Does_Not_Exist()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await TestDataSeeder.SeedUserAsync(factory);
		var client = factory.CreateAuthenticatedClient(userId);

		var response = await client.DeleteAsync($"/api/asset-items/{Guid.NewGuid()}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}
}
