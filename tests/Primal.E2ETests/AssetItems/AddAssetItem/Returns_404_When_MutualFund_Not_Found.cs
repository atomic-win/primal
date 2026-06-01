using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.AssetItems.AddAssetItem;

public sealed class Returns_404_When_MutualFund_Not_Found
{
	[Test]
	public async Task Test()
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
