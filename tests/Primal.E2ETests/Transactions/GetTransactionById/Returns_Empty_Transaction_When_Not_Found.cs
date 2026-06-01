using System.Net;
using System.Net.Http.Json;
using Primal.Api.AssetItems;

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

		var createResponse = await client.PostAsJsonAsync("/api/asset-items", new
		{
			Name = "Test Fixed Deposit",
			AssetClass = "Debt",
			AssetType = "FixedDeposit",
			ExternalId = string.Empty,
			Currency = "INR",
		});
		var assetItem = await createResponse.ReadJsonAsync<AssetItemResponse>();

		var response = await client.GetAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{Guid.NewGuid()}?currency=INR");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
