using System.Net;

namespace Primal.E2ETests.Transactions.GetTransactionById;

public sealed class Returns_404_When_TransactionId_Is_Not_Guid
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "Test Bank",
			assetClass: "EmergencyFund",
			assetType: "BankAccount",
			externalId: string.Empty,
			currency: "INR");

		// Act
		var response = await client.GetAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/not-a-guid?currency=INR");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}
}
