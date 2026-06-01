using System.Net;
using System.Net.Http.Json;
using Primal.Api.AssetItems;

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

		var createResponse = await client.PostAsJsonAsync("/api/asset-items", new
		{
			Name = "Test Fixed Deposit",
			AssetClass = "Debt",
			AssetType = "FixedDeposit",
			ExternalId = string.Empty,
			Currency = "INR",
		});
		var assetItem = await createResponse.ReadJsonAsync<AssetItemResponse>();

		var response = await client.DeleteAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{Guid.NewGuid()}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}
}
