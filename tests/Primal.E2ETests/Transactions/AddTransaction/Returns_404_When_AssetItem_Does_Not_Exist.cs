using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.Transactions.AddTransaction;

public sealed class Returns_404_When_AssetItem_Does_Not_Exist
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var nonExistentAssetItemId = Guid.NewGuid();

		// Act
		var response = await client.PostAsJsonAsync(
			$"/api/asset-items/{nonExistentAssetItemId}/transactions", new
			{
				AssetItemId = nonExistentAssetItemId,
				Date = "2026-01-15",
				Name = "Test Transaction",
				TransactionType = "Buy",
				Units = 10.5,
				Price = 150.25,
				Amount = 0,
			});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}
}
