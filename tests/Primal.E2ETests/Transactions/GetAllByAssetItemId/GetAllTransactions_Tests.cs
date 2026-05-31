using System.Net;
using VerifyTUnit;

namespace Primal.E2ETests.Transactions.GetAllByAssetItemId;

public sealed class GetAllTransactions_Tests
{
	[Test]
	public async Task Returns_Transactions_For_AssetItem()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		WireMockSetup.SetupMutualFundLatest(factory.MutualFundApi);
		WireMockSetup.SetupMutualFundPrices(factory.MutualFundApi);

		var userId = await TestDataSeeder.SeedUserAsync(factory);
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

	[Test]
	public async Task Returns_404_When_AssetItem_Does_Not_Exist()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await TestDataSeeder.SeedUserAsync(factory);
		var client = factory.CreateAuthenticatedClient(userId);

		var response = await client.GetAsync(
			$"/api/asset-items/{Guid.NewGuid()}/transactions?currency=INR");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}
}
