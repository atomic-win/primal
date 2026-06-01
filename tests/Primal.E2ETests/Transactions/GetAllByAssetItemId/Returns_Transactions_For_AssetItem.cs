using System.Net;

namespace Primal.E2ETests.Transactions.GetAllByAssetItemId;

public sealed class Returns_Transactions_For_AssetItem
{
	[Test]
	public async Task Test()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		factory.MutualFundApi.SetupMutualFundLatest(schemeCode: "119551");
		factory.MutualFundApi.SetupMutualFundPrices(schemeCode: "119551");

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);
		var assetItemId = await TestDataSeeder.SeedAssetItemViaMutualFundAsync(client);

		await TestDataSeeder.SeedBuyTransactionAsync(client, assetItemId, "2026-01-15", "Buy Units Batch 1", 10.0m, 150.25m);
		await TestDataSeeder.SeedBuyTransactionAsync(client, assetItemId, "2026-01-16", "Buy Units Batch 2", 5.0m, 151.00m);

		var response = await client.GetAsync(
			$"/api/asset-items/{assetItemId}/transactions?currency=INR");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
