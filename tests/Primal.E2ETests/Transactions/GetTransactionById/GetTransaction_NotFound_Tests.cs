using System.Net;
using VerifyTUnit;

namespace Primal.E2ETests.Transactions.GetTransactionById;

public sealed class GetTransaction_NotFound_Tests
{
	[Test]
	public async Task Returns_Empty_Transaction_When_Not_Found()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		WireMockSetup.SetupExchangeRate(factory.ExchangeRateApi);

		var userId = await TestDataSeeder.SeedUserAsync(factory);
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItemId = await TestDataSeeder.SeedAssetItemViaFixedDepositAsync(client);

		var response = await client.GetAsync(
			$"/api/asset-items/{assetItemId}/transactions/{Guid.NewGuid()}?currency=INR");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
