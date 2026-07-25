using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace InvestmentPortfolioTracker.E2ETests.Users.GetUser;

public sealed class Returns_401_When_JWT_Is_Expired
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new InvestmentPortfolioTrackerE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();

		var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("super-secret-test-key-that-is-long-enough-for-hmac-sha256"));
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: "TestIssuer",
			audience: "TestAudience",
			claims: [new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString("D"))],
			expires: DateTime.UtcNow.AddMinutes(-10),
			signingCredentials: credentials);

		var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);

		// Act
		var response = await client.GetAsync("/api/users/me");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
