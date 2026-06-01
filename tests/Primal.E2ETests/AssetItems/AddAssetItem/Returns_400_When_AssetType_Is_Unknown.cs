using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.AssetItems.AddAssetItem;

public sealed class Returns_400_When_AssetType_Is_Unknown
{
	[Test]
	public async Task Test()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var response = await client.PostAsJsonAsync("/api/asset-items", new
		{
			Name = "Test Asset",
			AssetClass = "Equity",
			AssetType = "Unknown",
			ExternalId = string.Empty,
			Currency = "INR",
		});

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
