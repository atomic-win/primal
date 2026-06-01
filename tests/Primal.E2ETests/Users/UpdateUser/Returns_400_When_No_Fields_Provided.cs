using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.Users.UpdateUser;

public sealed class Returns_400_When_No_Fields_Provided
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
		var response = await client.PatchAsJsonAsync("/api/users/me", new
		{
			PreferredCurrency = "Unknown",
			PreferredLocale = "Unknown",
		});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
