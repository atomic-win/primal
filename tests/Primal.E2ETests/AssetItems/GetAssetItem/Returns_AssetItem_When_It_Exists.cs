using System.Net;

namespace Primal.E2ETests.AssetItems.GetAssetItem;

public sealed class Returns_AssetItem_When_It_Exists
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

		var response = await client.GetAsync($"/api/asset-items/{assetItemId}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
