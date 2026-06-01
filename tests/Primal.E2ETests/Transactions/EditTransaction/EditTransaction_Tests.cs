using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.Transactions.EditTransaction;

public sealed class EditTransaction_Tests
{
	[Test]
	public async Task Returns_204_And_GET_Returns_Updated_Transaction()
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

		var editResponse = await client.PatchAsJsonAsync(
			$"/api/asset-items/{assetItemId}/transactions/{transactionId}",
			new
			{
				AssetItemId = assetItemId,
				TransactionId = transactionId,
				Name = "Buy Units - Updated",
				TransactionType = "Unknown",
				Units = 15.0,
				Price = 155.50,
				Amount = 0,
			});

		await Assert.That(editResponse.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

		var getResponse = await client.GetAsync(
			$"/api/asset-items/{assetItemId}/transactions/{transactionId}?currency=INR");
		var body = await getResponse.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
