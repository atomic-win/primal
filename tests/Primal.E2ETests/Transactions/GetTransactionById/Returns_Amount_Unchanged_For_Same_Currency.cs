using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Primal.Api.Transactions;

namespace Primal.E2ETests.Transactions.GetTransactionById;

public sealed class Returns_Amount_Unchanged_For_Same_Currency
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() },
	};

	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "INR Wallet",
			assetClass: "EmergencyFund",
			assetType: "Wallet",
			externalId: string.Empty,
			currency: "INR");

		var transaction = await client.AddTransactionAsync(
			assetItemId: assetItem.Id,
			date: "2026-01-15",
			name: "Deposit",
			transactionType: "Deposit",
			units: 0m,
			price: 0m,
			amount: 123.45m);

		// Act — request with same currency as asset
		var response = await client.GetAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{transaction.Id}?currency=INR");

		// Assert — amount should be unchanged (exchange rate = 1)
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadFromJsonAsync<TransactionResponse>(JsonOptions);
		await Assert.That(body!.Amount).IsEqualTo(123.45m);
	}
}
