using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.Users.UpdateUser;

public sealed class UpdateUser_Unauthenticated_Tests
{
	[Test]
	public async Task Returns_401_When_No_Token_Provided()
	{
		await using var factory = new PrimalE2EFactory();
		var client = factory.CreateClient();

		var response = await client.PatchAsJsonAsync("/api/users/me", new
		{
			PreferredCurrency = "INR",
			PreferredLocale = "Unknown",
		});

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
	}
}
