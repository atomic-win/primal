using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.Transactions.AddTransaction;

public sealed class Returns_400_When_AssetItemId_Is_Empty
{
	[Test]
	public async Task Test()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
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
}
