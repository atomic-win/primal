using System.Net;
using System.Net.Http.Json;

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

		var assetItem = await client.AddAssetItemAsync(
			name: "Test Mutual Fund",
			assetClass: "Equity",
			assetType: "MutualFund",
			externalId: "119551",
			currency: "Unknown");

		var response = await client.PostAsJsonAsync(
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

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
