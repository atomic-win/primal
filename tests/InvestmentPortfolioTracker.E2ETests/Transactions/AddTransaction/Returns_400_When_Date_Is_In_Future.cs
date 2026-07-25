using System.Net;
using System.Net.Http.Json;

namespace InvestmentPortfolioTracker.E2ETests.Transactions.AddTransaction;

public sealed class Returns_400_When_Date_Is_In_Future
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

		var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

		// Act
		var response = await client.PostAsJsonAsync(
			$"/api/asset-items/{assetItem.Id}/transactions", new
			{
				AssetItemId = assetItem.Id,
				Date = futureDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
				Name = "Deposit",
				TransactionType = "Deposit",
				Units = 0,
				Price = 0,
				Amount = 100,
			});

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
