using System.Net;

namespace Primal.E2ETests.Transactions.GetTransactionById;

public sealed class Returns_Empty_Transaction_When_Not_Found
{
	[Test]
	public async Task Test()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItemId = await TestDataSeeder.SeedAssetItemViaFixedDepositAsync(client);

		var response = await client.GetAsync(
			$"/api/asset-items/{assetItemId}/transactions/{Guid.NewGuid()}?currency=INR");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
