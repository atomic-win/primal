using System.Net;

namespace Primal.E2ETests.Auth.RefreshToken;

public sealed class Returns_415_When_No_Body
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		var client = factory.CreateClient();

		// Act
		var response = await client.PostAsync("/api/auth/refresh-token", null);

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.UnsupportedMediaType);
	}
}
