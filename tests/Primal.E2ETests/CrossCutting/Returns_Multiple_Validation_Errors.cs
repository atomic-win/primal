using System.Net;
using System.Net.Http.Json;

namespace Primal.E2ETests.CrossCutting;

public sealed class Returns_Multiple_Validation_Errors
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
			AssetClass = "Equity",
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
