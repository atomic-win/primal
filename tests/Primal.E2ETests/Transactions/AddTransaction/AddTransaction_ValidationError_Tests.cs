using System.Net;
using System.Net.Http.Json;
using VerifyTUnit;

namespace Primal.E2ETests.Transactions.AddTransaction;

public sealed class AddTransaction_ValidationError_Tests
{
	[Test]
	public async Task Returns_400_When_AssetItemId_Is_Empty()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await TestDataSeeder.SeedUserAsync(factory);
		var client = factory.CreateAuthenticatedClient(userId);

		var response = await client.PostAsJsonAsync($"/api/asset-items/{Guid.Empty}/transactions", new
		{
			AssetItemId = Guid.Empty,
			Date = "2026-01-15",
			Name = "Test Transaction",
			TransactionType = "Buy",
			Units = 10.5,
			Price = 150.25,
			Amount = 0,
		});

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}

	[Test]
	public async Task Returns_400_When_Name_Is_Too_Short()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await TestDataSeeder.SeedUserAsync(factory);
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
