using System.Net;

namespace Primal.E2ETests.Transactions.DeleteTransaction;

public sealed class Returns_500_When_Transaction_Does_Not_Exist
{
	[Test]
	public async Task Test()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "Test Fixed Deposit",
			assetClass: "Debt",
			assetType: "FixedDeposit",
			externalId: string.Empty,
			currency: "INR");

		var response = await client.DeleteAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{Guid.NewGuid()}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}
}
