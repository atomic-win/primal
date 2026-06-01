using System.Net;

namespace Primal.E2ETests.Users.UpdateUser;

public sealed class Returns_400_When_No_Body
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		// Act
		var response = await client.PatchAsync("/api/users/me", null);

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.UnsupportedMediaType);
	}
}
