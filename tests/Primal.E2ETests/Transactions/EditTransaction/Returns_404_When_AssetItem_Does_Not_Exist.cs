using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.Transactions.EditTransaction;

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
		var transactionId = Guid.NewGuid();

		// Act
		var response = await client.PatchAsJsonAsync(
			$"/api/asset-items/{nonExistentAssetItemId}/transactions/{transactionId}",
			new
			{
				AssetItemId = nonExistentAssetItemId,
				TransactionId = transactionId,
				Name = "Updated Name",
				TransactionType = "Unknown",
				Units = 10.0,
				Price = 150.25,
				Amount = 0,
			});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
