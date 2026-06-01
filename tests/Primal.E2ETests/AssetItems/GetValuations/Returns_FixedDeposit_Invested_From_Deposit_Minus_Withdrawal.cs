using System.Net;

namespace Primal.E2ETests.AssetItems.GetValuations;

public sealed class Returns_FixedDeposit_Invested_From_Deposit_Minus_Withdrawal
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
			"Test FD", "Debt", "FixedDeposit", string.Empty, "INR");

		await client.AddTransactionAsync(
			assetItem.Id,
			"2026-05-15",
			"Initial Deposit",
			"Deposit",
			0,
			0,
			10000);

		await client.AddTransactionAsync(
			assetItem.Id,
			"2026-05-20",
			"Partial Withdrawal",
			"Withdrawal",
			0,
			0,
			3000);

		// Act
		var response = await client.GetAsync(
			$"/api/asset-items/valuations?currency=INR&assetItemIds={assetItem.Id}");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
