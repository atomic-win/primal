using System.Net;

namespace Primal.E2ETests.Users.GetUser;

public sealed class GetUser_Unauthenticated_Tests
{
	[Test]
	public async Task Returns_401_When_No_Token_Provided()
	{
		await using var factory = new PrimalE2EFactory();
		var client = factory.CreateClient();

		var response = await client.GetAsync("/api/users/me");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
	}
}
