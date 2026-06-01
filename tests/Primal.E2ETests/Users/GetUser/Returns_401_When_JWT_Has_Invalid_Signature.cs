using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Primal.E2ETests.Users.GetUser;

public sealed class Returns_401_When_JWT_Has_Invalid_Signature
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();

		var wrongKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("a-completely-different-secret-key-that-is-long-enough!!"));
		var credentials = new SigningCredentials(wrongKey, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: "TestIssuer",
			audience: "TestAudience",
			claims: [new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString("D"))],
			expires: DateTime.UtcNow.AddMinutes(60),
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
