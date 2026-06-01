using System.Net;
using System.Net.Http.Json;
using Primal.Api.AssetItems;
using Primal.Api.Transactions;

namespace Primal.E2ETests.Transactions.AddTransaction;

public sealed class Returns_201_And_GET_Returns_Created_Transaction
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

		var createAssetResponse = await client.PostAsJsonAsync("/api/asset-items", new
		{
			Name = "Test Mutual Fund",
			AssetClass = "Equity",
			AssetType = "MutualFund",
			ExternalId = "119551",
			Currency = "Unknown",
		});
		var assetItem = await createAssetResponse.ReadJsonAsync<AssetItemResponse>();

		var createTxResponse = await client.PostAsJsonAsync(
			$"/api/asset-items/{assetItem.Id}/transactions", new
			{
				AssetItemId = assetItem.Id,
				Date = "2026-01-15",
				Name = "Buy Mutual Fund Units",
				TransactionType = "Buy",
				Units = 10.5m,
				Price = 150.25m,
				Amount = 0,
			});
		var transaction = await createTxResponse.ReadJsonAsync<TransactionResponse>();

		// Validate via GET
		var getResponse = await client.GetAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{transaction.Id}?currency=INR");

		await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await getResponse.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
