using System.Net;
using System.Net.Http.Json;

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

		var assetItemId = await TestDataSeeder.SeedAssetItemViaFixedDepositAsync(client);

		var response = await client.PostAsJsonAsync(
			$"/api/asset-items/{assetItemId}/transactions", new
			{
				AssetItemId = assetItemId,
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
