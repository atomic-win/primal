using System.Net;
using System.Net.Http.Json;
using Google.Apis.Auth;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Primal.E2ETests.Auth.GoogleLogin;

public sealed class Returns_401_When_Google_Token_Expired
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		var client = factory.CreateClient();

		factory.IdTokenValidator
			.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.ThrowsAsync(new InvalidJwtException("JWT has expired."));

		// Act
		var response = await client.PostAsJsonAsync("/api/auth/login/google", new
		{
			IdToken = "expired-token",
		});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
