using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.CrossCutting;

public sealed class Returns_ProblemDetails_On_Validation_Error
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
		var response = await client.PostAsJsonAsync("/api/asset-items", new
		{
			Name = string.Empty,
			AssetClass = "Unknown",
			AssetType = "Unknown",
			ExternalId = string.Empty,
			Currency = "Unknown",
		});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
