using System.Net;

namespace Primal.E2ETests.AssetItems.GetAssetItem;

public sealed class Returns_404_When_AssetItem_Does_Not_Exist
{
	[Test]
	public async Task Test()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var response = await client.GetAsync($"/api/asset-items/{Guid.NewGuid()}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}
}
