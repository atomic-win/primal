using System.Net;

namespace Primal.E2ETests.AssetItems.GetValuations;

public sealed class Returns_400_When_AssetItem_Not_Found
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);
		var nonExistentId = Guid.NewGuid();

		// Act
		var response = await client.GetAsync($"/api/asset-items/valuations?currency=USD&assetItemIds={nonExistentId}");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
