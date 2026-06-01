using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Primal.Api.Transactions;

namespace Primal.E2ETests.Transactions.GetTransactionById;

public sealed class Returns_Units_Multiplied_By_Exchange_Rate_For_BankAccount
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

		factory.ExchangeRateApi.SetupExchangeRate(date: "2026-01-15", closeRate: 2m);

		var userId = await factory.CreateUserAsync();
		var client = factory.CreateAuthenticatedClient(userId);

		var assetItem = await client.AddAssetItemAsync(
			name: "USD Savings",
			assetClass: "Debt",
			assetType: "BankAccount",
			externalId: string.Empty,
			currency: "USD");

		var transaction = await client.AddTransactionAsync(
			assetItemId: assetItem.Id,
			date: "2026-01-15",
			name: "Deposit",
			transactionType: "Deposit",
			units: 0m,
			price: 0m,
			amount: 50m);

		// Act — request in INR (asset is USD, exchange rate = 2)
		var response = await client.GetAsync(
			$"/api/asset-items/{assetItem.Id}/transactions/{transaction.Id}?currency=INR");

		// Assert — 50 USD * 2 = 100 INR
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadFromJsonAsync<TransactionResponse>(JsonOptions);
		await Assert.That(body!.Amount).IsEqualTo(100m);
	}
}
