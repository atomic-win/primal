using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.Auth.GoogleLogin;

public sealed class Returns_Reachable_Without_Auth
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		var client = factory.CreateClient();

		// Act
		var response = await client.PostAsJsonAsync("/api/auth/login/google", new
		{
			IdToken = string.Empty,
		});

		// Assert
		await Assert.That(response.StatusCode).IsNotEqualTo(HttpStatusCode.Unauthorized);
	}
}
