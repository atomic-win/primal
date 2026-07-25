using System.Net;

namespace InvestmentPortfolioTracker.E2ETests.Transactions.GetTransactionById;

public sealed class Returns_List_When_TransactionId_Missing
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new InvestmentPortfolioTrackerE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "Test Bank",
			assetClass: "EmergencyFund",
			assetType: "BankAccount",
			externalId: string.Empty,
			currency: "INR");

		// Act — trailing slash with no transactionId should match the list endpoint
		var response = await client.GetAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/?currency=INR");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
