using System.Net;

namespace Primal.E2ETests.Auth.GoogleLogin;

public sealed class Returns_400_When_No_Body
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		var client = factory.CreateClient();

		// Act
		var response = await client.PostAsync("/api/auth/login/google", null);

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.UnsupportedMediaType);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
