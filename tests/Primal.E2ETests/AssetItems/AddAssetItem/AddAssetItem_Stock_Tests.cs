using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.AssetItems.AddAssetItem;

public sealed class AddAssetItem_Stock_Tests
{
	[Test]
	public async Task Returns_201_When_Adding_Valid_Stock()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		WireMockSetup.SetupStockSearch(factory.StockApi);

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var response = await client.PostAsJsonAsync("/api/asset-items", new
		{
			Name = "Apple Stock",
			AssetClass = "Unknown",
			AssetType = "Stock",
			ExternalId = "AAPL",
			Currency = "Unknown",
		});

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
	}

	[Test]
	public async Task Returns_404_When_Stock_Not_Found()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		WireMockSetup.SetupStockSearchEmpty(factory.StockApi);

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var response = await client.PostAsJsonAsync("/api/asset-items", new
		{
			Name = "Invalid Stock",
			AssetClass = "Unknown",
			AssetType = "Stock",
			ExternalId = "INVALID",
			Currency = "Unknown",
		});

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}
}
