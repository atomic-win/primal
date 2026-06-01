using System.Net;
using System.Net.Http.Json;
using Primal.Api.AssetItems;
using Primal.Api.Transactions;

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

		var createAssetResponse = await client.PostAsJsonAsync("/api/asset-items", new
		{
			Name = "Test Mutual Fund",
			AssetClass = "Equity",
			AssetType = "MutualFund",
			ExternalId = "119551",
			Currency = "Unknown",
		});
		var assetItem = await createAssetResponse.ReadJsonAsync<AssetItemResponse>();

		await client.PostAsJsonAsync(
			$"/api/asset-items/{assetItem.Id}/transactions", new
			{
				AssetItemId = assetItem.Id,
				Date = "2026-01-15",
				Name = "Buy Units Batch 1",
				TransactionType = "Buy",
				Units = 10.0m,
				Price = 150.25m,
				Amount = 0,
			});

		await client.PostAsJsonAsync(
			$"/api/asset-items/{assetItem.Id}/transactions", new
			{
				AssetItemId = assetItem.Id,
				Date = "2026-01-16",
				Name = "Buy Units Batch 2",
				TransactionType = "Buy",
				Units = 5.0m,
				Price = 151.00m,
				Amount = 0,
			});

		var response = await client.GetAsync(
			$"/api/asset-items/{assetItem.Id}/transactions?currency=INR");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
