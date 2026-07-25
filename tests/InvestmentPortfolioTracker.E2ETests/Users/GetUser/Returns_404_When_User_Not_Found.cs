using System.Net;

using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.E2ETests.Users.GetUser;

public sealed class Returns_404_When_User_Not_Found
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new InvestmentPortfolioTrackerE2EFactory();
		_ = factory.CreateClient();

		var userId = new UserId(Guid.NewGuid());
		var client = factory.CreateAuthenticatedClient(userId);

		// Act
		var response = await client.GetAsync("/api/users/me");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
