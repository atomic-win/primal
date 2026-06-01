using System.Net;
using System.Net.Http.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Primal.E2ETests.Auth.GoogleLogin;

public sealed class Returns_500_When_Unexpected_Error
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		var client = factory.CreateClient();

		factory.IdTokenValidator
			.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.ThrowsAsync(new InvalidOperationException("Unexpected error from Google"));

		// Act
		var response = await client.PostAsJsonAsync("/api/auth/login/google", new
		{
			IdToken = "some-token",
		});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
