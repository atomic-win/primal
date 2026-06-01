using System.Net;
using Primal.Domain.Users;

namespace Primal.E2ETests.Users.GetUser;

public sealed class GetUser_Authenticated_Tests
{
	[Test]
	public async Task Returns_User_When_Authenticated()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var response = await client.GetAsync("/api/users/me");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}

	[Test]
	public async Task Returns_404_When_User_Not_Found()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = new UserId(Guid.NewGuid());
		var client = factory.CreateAuthenticatedClient(userId);

		var response = await client.GetAsync("/api/users/me");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}
}
