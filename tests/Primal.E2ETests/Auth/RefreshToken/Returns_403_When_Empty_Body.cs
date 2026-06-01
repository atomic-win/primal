using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.Auth.RefreshToken;

public sealed class Returns_403_When_Empty_Body
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		var client = factory.CreateClient();

		// Act
		var response = await client.PostAsJsonAsync("/api/auth/refresh-token", new { });

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
