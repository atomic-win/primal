using System.Net;
using System.Net.Http.Json;

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

		await client.PostAsJsonAsync("/api/asset-items", new
		{
			Name = "My Equity Fund",
			AssetClass = "Equity",
			AssetType = "MutualFund",
			ExternalId = "119551",
			Currency = "Unknown",
		});

		var response = await client.GetAsync("/api/asset-items");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
