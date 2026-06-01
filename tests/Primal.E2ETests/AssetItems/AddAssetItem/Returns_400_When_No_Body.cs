using System.Net;

namespace Primal.E2ETests.AssetItems.AddAssetItem;

public sealed class Returns_400_When_No_Body
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		// Act
		var response = await client.PostAsync("/api/asset-items", null);

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.UnsupportedMediaType);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
