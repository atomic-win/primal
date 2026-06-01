using System.Net;
using System.Net.Http.Json;
using Primal.Api.AssetItems;

namespace Primal.E2ETests.Transactions.AddTransaction;

public sealed class Returns_400_When_Name_Is_Too_Short
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

		var response = await client.PostAsJsonAsync(
			$"/api/asset-items/{assetItem.Id}/transactions", new
			{
				AssetItemId = assetItem.Id,
				Date = "2026-01-15",
				Name = "AB",
				TransactionType = "Deposit",
				Units = 0,
				Price = 0,
				Amount = 1000,
			});

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
