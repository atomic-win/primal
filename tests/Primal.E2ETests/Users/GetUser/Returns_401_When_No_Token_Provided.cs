using System.Net;

namespace Primal.E2ETests.Users.GetUser;

public sealed class Returns_401_When_No_Token_Provided
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		var client = factory.CreateClient();

		// Act
		var response = await client.GetAsync("/api/users/me");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
	}
}
