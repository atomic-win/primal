using System.Net;
using Primal.Domain.Users;

namespace Primal.E2ETests.Users.GetUser;

public sealed class Returns_404_When_User_Not_Found
{
	[Test]
	public async Task Test()
	{
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = new UserId(Guid.NewGuid());
		var client = factory.CreateAuthenticatedClient(userId);

		var response = await client.GetAsync("/api/users/me");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}
}
