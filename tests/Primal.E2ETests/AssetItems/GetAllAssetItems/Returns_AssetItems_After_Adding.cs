using System.Net;

namespace Primal.E2ETests.AssetItems.GetAllAssetItems;

public sealed class Returns_AssetItems_After_Adding
{
	[Test]
	public async Task Test()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		factory.MutualFundApi.SetupMutualFundLatest(schemeCode: "119551");

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		await client.AddAssetItemAsync(
			name: "My Equity Fund",
			assetClass: "Equity",
			assetType: "MutualFund",
			externalId: "119551",
			currency: "Unknown");

		var response = await client.GetAsync("/api/asset-items");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
