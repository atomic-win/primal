using System.Net;

namespace Primal.E2ETests.AssetItems.GetAssetItem;

public sealed class Returns_AssetItem_When_It_Exists
{
	[Test]
	public async Task Test()
	{
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

		var response = await client.GetAsync($"/api/asset-items/{assetItem.Id}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
