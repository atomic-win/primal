using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.Users.UpdateUser;

public sealed class Returns_401_When_No_Token_Provided
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		var client = factory.CreateClient();

		// Act
		var response = await client.PatchAsJsonAsync("/api/users/me", new
		{
			PreferredCurrency = "INR",
			PreferredLocale = "Unknown",
		});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
