using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.Transactions.EditTransaction;

public sealed class Returns_404_When_Transaction_Does_Not_Exist
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		factory.MutualFundApi.SetupMutualFundLatest(schemeCode: "119551");

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "Test Mutual Fund",
			assetClass: "Equity",
			assetType: "MutualFund",
			externalId: "119551",
			currency: "Unknown");

		var nonExistentTransactionId = Guid.NewGuid();

		// Act
		var response = await client.PatchAsJsonAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{nonExistentTransactionId}",
			new
			{
				AssetItemId = assetItem.Id,
				TransactionId = nonExistentTransactionId,
				Name = "Updated Name",
				TransactionType = "Unknown",
				Units = 10.0,
				Price = 150.25,
				Amount = 0,
			});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
