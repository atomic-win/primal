using System.Net;
using System.Net.Http.Json;

namespace InvestmentPortfolioTracker.E2ETests.Transactions.EditTransaction;

public sealed class Returns_404_When_Route_Params_Not_Guid
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new InvestmentPortfolioTrackerE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		// Act
		var response = await client.PatchAsJsonAsync(
			"/api/asset-items/not-a-guid/transactions/not-a-guid",
			new
			{
				Name = "Test",
				TransactionType = "Unknown",
				Units = 0,
				Price = 0,
				Amount = 0,
			});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
