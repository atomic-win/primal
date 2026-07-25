using System.Net;
using System.Net.Http.Json;

using NSubstitute;

using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.E2ETests.Auth.GoogleLogin;

public sealed class Returns_200_When_New_User_Registers
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new InvestmentPortfolioTrackerE2EFactory();
		var client = factory.CreateClient();

		factory.IdTokenValidator
			.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(new IdentityProviderUser(
				id: new IdentityProviderUserId("google-subject-123"),
				identityProvider: IdentityProvider.Google,
				email: "newuser@example.com",
				firstName: "New",
				lastName: "User",
				fullName: "New User",
				profilePictureUrl: new Uri("https://example.com/photo.jpg")));

		// Act
		var response = await client.PostAsJsonAsync("/api/auth/login/google", new
		{
			IdToken = "valid-google-token",
		});

		// Assert
		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
