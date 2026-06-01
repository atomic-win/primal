using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.AssetItems.AddAssetItem;

public sealed class AddAssetItem_MutualFund_Tests
{
	[Test]
	public async Task Returns_201_When_Adding_Valid_MutualFund()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		WireMockSetup.SetupMutualFundLatest(factory.MutualFundApi);

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var response = await client.PostAsJsonAsync("/api/asset-items", new
		{
			Name = "My Equity Fund",
			AssetClass = "Equity",
			AssetType = "MutualFund",
			ExternalId = "119551",
			Currency = "Unknown",
		});

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
	}

	[Test]
	public async Task Returns_404_When_MutualFund_Not_Found()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		WireMockSetup.SetupMutualFundNotFound(factory.MutualFundApi, "999999");

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var response = await client.PostAsJsonAsync("/api/asset-items", new
		{
			Name = "Invalid Fund",
			AssetClass = "Equity",
			AssetType = "MutualFund",
			ExternalId = "999999",
			Currency = "Unknown",
		});

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}
}
