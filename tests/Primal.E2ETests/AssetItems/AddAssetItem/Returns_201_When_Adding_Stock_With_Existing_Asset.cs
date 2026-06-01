using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.AssetItems.AddAssetItem;

public sealed class Returns_201_When_Adding_Stock_With_Existing_Asset
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		factory.StockApi.SetupStockSearch(symbol: "AAPL");

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		await client.AddAssetItemAsync(
			name: "First AAPL",
			assetClass: "Unknown",
			assetType: "Stock",
			externalId: "AAPL",
			currency: "Unknown");

		// Act
		var response = await client.PostAsJsonAsync("/api/asset-items", new
		{
			Name = "Second AAPL",
			AssetClass = "Unknown",
			AssetType = "Stock",
			ExternalId = "AAPL",
			Currency = "Unknown",
		});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
