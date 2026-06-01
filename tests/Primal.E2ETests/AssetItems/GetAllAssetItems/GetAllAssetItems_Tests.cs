using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.AssetItems.GetAllAssetItems;

public sealed class GetAllAssetItems_Tests
{
	[Test]
	public async Task Returns_Empty_List_When_No_AssetItems()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var response = await client.GetAsync("/api/asset-items");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}

	[Test]
	public async Task Returns_AssetItems_After_Adding()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		WireMockSetup.SetupMutualFundLatest(factory.MutualFundApi);

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

	[Test]
	public async Task Returns_401_When_Unauthenticated()
	{
		await using var factory = new PrimalE2EFactory();
		var client = factory.CreateClient();

		var response = await client.GetAsync("/api/asset-items");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
	}
}
