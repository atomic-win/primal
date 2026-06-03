using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.AssetItems.EditAssetItem;

public sealed class Returns_204_When_Name_Is_Unchanged
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		factory.MutualFundApi.SetupMutualFundLatest(schemeCode: "119551");

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "Test Mutual Fund",
			assetClass: "Equity",
			assetType: "MutualFund",
			externalId: "119551",
			currency: "Unknown");

		// Act
		var response = await client.PatchAsJsonAsync(
			$"/api/asset-items/{assetItem.Id}",
			new { Name = "Test Mutual Fund" });

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

		var getResponse = await client.GetAsync($"/api/asset-items/{assetItem.Id}");
		var body = await getResponse.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
