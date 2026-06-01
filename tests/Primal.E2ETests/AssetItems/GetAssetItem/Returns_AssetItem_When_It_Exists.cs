using System.Net;
using System.Net.Http.Json;
using Primal.Api.AssetItems;

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

		var createResponse = await client.PostAsJsonAsync("/api/asset-items", new
		{
			Name = "Test Mutual Fund",
			AssetClass = "Equity",
			AssetType = "MutualFund",
			ExternalId = "119551",
			Currency = "Unknown",
		});
		var assetItem = await createResponse.ReadJsonAsync<AssetItemResponse>();

		var response = await client.GetAsync($"/api/asset-items/{assetItem.Id}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
