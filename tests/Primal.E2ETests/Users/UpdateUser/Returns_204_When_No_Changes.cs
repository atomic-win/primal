using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.Users.UpdateUser;

public sealed class Returns_204_When_No_Changes
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
			PreferredCurrency = "USD",
			PreferredLocale = "EN_US",
		});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

		// Verify the user is unchanged
		var getResponse = await client.GetAsync("/api/users/me");
		var body = await getResponse.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
