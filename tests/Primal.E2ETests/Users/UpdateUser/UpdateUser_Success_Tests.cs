using System.Net;
using System.Net.Http.Json;
using VerifyTUnit;

namespace Primal.E2ETests.Users.UpdateUser;

public sealed class UpdateUser_Success_Tests
{
	[Test]
	public async Task Returns_204_And_GET_Returns_Updated_User()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await TestDataSeeder.SeedUserAsync(factory);
		var client = factory.CreateAuthenticatedClient(userId);

		var response = await client.PatchAsJsonAsync("/api/users/me", new
		{
			PreferredCurrency = "INR",
			PreferredLocale = "Unknown",
		});

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

		// Validate via GET
		var getResponse = await client.GetAsync("/api/users/me");
		var body = await getResponse.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
