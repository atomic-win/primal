using System.Net;
using System.Net.Http.Json;
using Primal.Api.AssetItems;

namespace Primal.E2ETests.AssetItems.GetValuations;

public sealed class Returns_Valuations_For_Valid_Request
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		// Create a BankAccount asset item (no external API needed)
		var assetItem = await client.AddAssetItemAsync(
			"My Bank", "EmergencyFund", "BankAccount", string.Empty, "INR");

		// Act — request valuations with same currency as asset (no exchange rate needed)
		var response = await client.GetAsync(
			$"/api/asset-items/valuations?currency=INR&assetItemIds={assetItem.Id}");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var valuations = await response.Content.ReadFromJsonAsync<ValuationResponse[]>();
		await Assert.That(valuations).IsNotNull();
		await Assert.That(valuations!.Length).IsGreaterThanOrEqualTo(1);
		await Assert.That(valuations[0].InvestedValue).IsEqualTo(0m);
		await Assert.That(valuations[0].CurrentValue).IsEqualTo(0m);
	}
}
