using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.AssetItems.AddAssetItem;

public sealed class Returns_404_When_Stock_Not_Found
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		factory.StockApi.SetupStockSearchEmpty();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		// Act
		var response = await client.PostAsJsonAsync("/api/asset-items", new
		{
			Name = "Invalid Stock",
			AssetClass = "Unknown",
			AssetType = "Stock",
			ExternalId = "INVALID",
			Currency = "Unknown",
		});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
