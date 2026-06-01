using System.Net;

namespace Primal.E2ETests.AssetItems.GetValuations;

public sealed class Returns_401_When_Unauthenticated
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		var client = factory.CreateClient();

		// Act
		var response = await client.GetAsync("/api/asset-items/valuations?currency=USD&assetItemIds=00000000-0000-0000-0000-000000000001");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
