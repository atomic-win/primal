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

		var assetItem = await client.AddAssetItemAsync(
			name: "Test Fixed Deposit",
			assetClass: "Debt",
			assetType: "FixedDeposit",
			externalId: string.Empty,
			currency: "INR");

		var response = await client.GetAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{Guid.NewGuid()}?currency=INR");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
