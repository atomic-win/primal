using System.Net;
using System.Net.Http.Json;

namespace InvestmentPortfolioTracker.E2ETests.Auth.GoogleLogin;

public sealed class Returns_400_When_Body_Is_Empty_Json
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new InvestmentPortfolioTrackerE2EFactory();
		var client = factory.CreateClient();

		// Act
		var response = await client.PostAsJsonAsync("/api/auth/login/google", new { });

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
