using System.Net;
using VerifyTUnit;

namespace Primal.E2ETests.Transactions.GetTransactionById;

public sealed class GetTransaction_Exists_Tests
{
	[Test]
	public async Task Returns_Transaction_When_It_Exists()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		WireMockSetup.SetupMutualFundLatest(factory.MutualFundApi);
		WireMockSetup.SetupMutualFundPrices(factory.MutualFundApi);

		var userId = await TestDataSeeder.SeedUserAsync(factory);
		var client = factory.CreateAuthenticatedClient(userId);
		var assetItemId = await TestDataSeeder.SeedAssetItemViaMutualFundAsync(client);
		var transactionId = await TestDataSeeder.SeedBuyTransactionAsync(
			client, assetItemId, "2026-01-15", "Buy Units", 10.0m, 150.25m);

		var response = await client.GetAsync(
			$"/api/asset-items/{assetItemId}/transactions/{transactionId}?currency=INR");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
