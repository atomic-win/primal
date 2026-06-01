using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.Transactions.EditTransaction;

public sealed class Returns_400_When_Units_Are_Negative
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		factory.MutualFundApi.SetupMutualFundLatest(schemeCode: "119551");
		factory.MutualFundApi.SetupMutualFundPrices(
			schemeCode: "119551",
			prices: [("15-01-2026", "150.25")]);

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "Test MF",
			assetClass: "Equity",
			assetType: "MutualFund",
			externalId: "119551",
			currency: "Unknown");

		var transaction = await client.AddTransactionAsync(
			assetItemId: assetItem.Id,
			date: "2026-01-15",
			name: "Buy Units",
			transactionType: "Buy",
			units: 10m,
			price: 150.25m,
			amount: 0m);

		// Act — negative units
		var response = await client.PatchAsJsonAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{transaction.Id}",
			new
			{
				AssetItemId = assetItem.Id,
				TransactionId = transaction.Id,
				Name = "Updated",
				TransactionType = "Buy",
				Units = -1,
				Price = 150.25,
				Amount = 0,
			});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
