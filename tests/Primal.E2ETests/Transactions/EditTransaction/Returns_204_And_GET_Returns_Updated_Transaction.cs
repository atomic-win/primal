using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.Transactions.EditTransaction;

public sealed class Returns_204_And_GET_Returns_Updated_Transaction
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
