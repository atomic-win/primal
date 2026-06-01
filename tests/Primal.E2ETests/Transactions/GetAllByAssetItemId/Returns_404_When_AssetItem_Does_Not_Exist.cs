using System.Net;

namespace Primal.E2ETests.Transactions.GetAllByAssetItemId;

public sealed class Returns_404_When_AssetItem_Does_Not_Exist
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
		var response = await client.GetAsync(
			$"/api/asset-items/{Guid.NewGuid()}/transactions?currency=INR");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}
}
