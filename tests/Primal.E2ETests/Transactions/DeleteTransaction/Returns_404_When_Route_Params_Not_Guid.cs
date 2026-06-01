using System.Net;

namespace Primal.E2ETests.Transactions.DeleteTransaction;

public sealed class Returns_404_When_Route_Params_Not_Guid
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		// Act
		var response = await client.DeleteAsync(
			"/api/asset-items/not-a-guid/transactions/not-a-guid");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
