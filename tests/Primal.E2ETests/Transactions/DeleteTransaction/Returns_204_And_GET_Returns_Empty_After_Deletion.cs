using System.Net;

namespace Primal.E2ETests.Transactions.DeleteTransaction;

public sealed class Returns_204_And_GET_Returns_Empty_After_Deletion
{
	[Test]
	public async Task Test()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		WireMockSetup.SetupMutualFundLatest(factory.MutualFundApi);
		WireMockSetup.SetupMutualFundPrices(factory.MutualFundApi);

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);
		var assetItemId = await TestDataSeeder.SeedAssetItemViaMutualFundAsync(client);
		var transactionId = await TestDataSeeder.SeedBuyTransactionAsync(
			client, assetItemId, "2026-01-15", "Buy Units", 10.0m, 150.25m);

		var response = await client.DeleteAsync(
			$"/api/asset-items/{assetItemId}/transactions/{transactionId}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

		// Validate via GET that transaction no longer exists
		var getResponse = await client.GetAsync(
			$"/api/asset-items/{assetItemId}/transactions/{transactionId}?currency=INR");
		var body = await getResponse.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
