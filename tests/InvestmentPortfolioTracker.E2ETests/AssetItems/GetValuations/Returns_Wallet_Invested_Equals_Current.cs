using System.Net;

namespace InvestmentPortfolioTracker.E2ETests.AssetItems.GetValuations;

public sealed class Returns_Wallet_Invested_Equals_Current
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
			"Test Wallet", "EmergencyFund", "Wallet", string.Empty, "INR");

		await client.AddTransactionAsync(
			assetItem.Id,
			"2026-05-15",
			"Initial Deposit",
			"Deposit",
			0,
			0,
			5000);

		await client.AddTransactionAsync(
			assetItem.Id,
			"2026-05-20",
			"Cash Withdrawal",
			"Withdrawal",
			0,
			0,
			1000);

		// Act
		var response = await client.GetAsync(
			$"/api/asset-items/valuations?currency=INR&assetItemIds={assetItem.Id}");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
