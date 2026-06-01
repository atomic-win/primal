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
		var assetItemId = await TestDataSeeder.SeedAssetItemViaFixedDepositAsync(client);

		var response = await client.DeleteAsync(
			$"/api/asset-items/{assetItemId}/transactions/{Guid.NewGuid()}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}
}
