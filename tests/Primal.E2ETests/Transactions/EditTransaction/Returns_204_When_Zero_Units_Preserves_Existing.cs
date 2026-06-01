using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.Transactions.EditTransaction;

public sealed class Returns_204_When_Zero_Units_Preserves_Existing
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
			prices: [("15-01-2026", "150.25"), ("16-01-2026", "151.00")]);

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
			units: 10.0m,
			price: 150.25m,
			amount: 0);

		// Act
		var editResponse = await client.PatchAsJsonAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{transaction.Id}",
			new
			{
				AssetItemId = assetItem.Id,
				TransactionId = transaction.Id,
				Name = "Updated Name",
				TransactionType = "Unknown",
				Units = 0,
				Price = 151.00,
				Amount = 0,
			});

		// Assert
		await Assert.That(editResponse.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

		var getResponse = await client.GetAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{transaction.Id}?currency=INR");
		var body = await getResponse.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
