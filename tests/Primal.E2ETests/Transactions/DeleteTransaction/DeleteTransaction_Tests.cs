using System.Net;
using VerifyTUnit;

namespace Primal.E2ETests.Transactions.DeleteTransaction;

public sealed class DeleteTransaction_Tests
{
	[Test]
	public async Task Returns_204_And_GET_Returns_Empty_After_Deletion()
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

		var response = await client.DeleteAsync(
			$"/api/asset-items/{assetItemId}/transactions/{transactionId}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

		// Validate via GET that transaction no longer exists
		var getResponse = await client.GetAsync(
			$"/api/asset-items/{assetItemId}/transactions/{transactionId}?currency=INR");
		var body = await getResponse.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}

	[Test]
	public async Task Returns_500_When_Transaction_Does_Not_Exist()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await TestDataSeeder.SeedUserAsync(factory);
		var client = factory.CreateAuthenticatedClient(userId);
		var assetItemId = await TestDataSeeder.SeedAssetItemViaFixedDepositAsync(client);

		var response = await client.DeleteAsync(
			$"/api/asset-items/{assetItemId}/transactions/{Guid.NewGuid()}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}
}
