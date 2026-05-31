using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using NSubstitute;
using Primal.Api.Transactions;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.IntegrationTests.Transactions;

public sealed class AddTransactionEndpointTests
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() },
	};

	[Test]
	public async Task AddTransaction_Unauthenticated_Returns401()
	{
		await using var factory = new PrimalApiFactory();
		var assetItemId = Guid.NewGuid();
		var client = factory.CreateClient();

		var request = new AddTransactionRequest(assetItemId, new DateOnly(2024, 6, 15), "Test transaction", TransactionType.Buy, 10m, 25m, 0m);
		var response = await client.PostAsJsonAsync($"/api/asset-items/{assetItemId}/transactions", request, JsonOptions);

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
	}

	[Test]
	[NotInParallel("AddTransaction")]
	public async Task AddTransaction_BuyTransaction_Returns201()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var assetId = new AssetId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());
		var transactionId = new TransactionId(Guid.NewGuid());
		var assetItem = new AssetItem(assetItemId, assetId, "My MF");
		var asset = new Asset(assetId, "Test MF", AssetClass.Equity, AssetType.MutualFund, Currency.USD, "mf-123456");
		var date = new DateOnly(2024, 6, 15);
		var transaction = new Transaction(transactionId, date, "SIP Buy", TransactionType.Buy, assetItemId, 10m, 25m, 0m);

		factory.AssetItemRepository
			.GetByIdAsync(userId, assetItemId, Arg.Any<CancellationToken>())
			.Returns(assetItem);

		factory.AssetRepository
			.GetByIdAsync(assetId, Arg.Any<CancellationToken>())
			.Returns(asset);

		factory.TransactionRepository
			.AddAsync(userId, assetItemId, date, "SIP Buy", TransactionType.Buy, 10m, 25m, 0m, Arg.Any<CancellationToken>())
			.Returns(transaction);

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var request = new AddTransactionRequest(assetItemId.Value, date, "SIP Buy", TransactionType.Buy, 10m, 25m, 0m);
		var response = await client.PostAsJsonAsync($"/api/asset-items/{assetItemId.Value}/transactions", request, JsonOptions);

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
	}

	[Test]
	[NotInParallel("AddTransaction")]
	public async Task AddTransaction_DepositTransaction_Returns201()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var assetId = new AssetId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());
		var transactionId = new TransactionId(Guid.NewGuid());
		var assetItem = new AssetItem(assetItemId, assetId, "My Bank");
		var asset = new Asset(assetId, "Test Bank", AssetClass.EmergencyFund, AssetType.BankAccount, Currency.USD, "bank-1");
		var date = new DateOnly(2024, 6, 15);

		factory.AssetItemRepository
			.GetByIdAsync(userId, assetItemId, Arg.Any<CancellationToken>())
			.Returns(assetItem);

		factory.AssetRepository
			.GetByIdAsync(assetId, Arg.Any<CancellationToken>())
			.Returns(asset);

		factory.TransactionRepository
			.AddAsync(userId, assetItemId, date, "Monthly deposit", TransactionType.Deposit, 0m, 0m, 500m, Arg.Any<CancellationToken>())
			.Returns(new Transaction(transactionId, date, "Monthly deposit", TransactionType.Deposit, assetItemId, 0m, 0m, 500m));

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var request = new AddTransactionRequest(assetItemId.Value, date, "Monthly deposit", TransactionType.Deposit, 0m, 0m, 500m);
		var response = await client.PostAsJsonAsync($"/api/asset-items/{assetItemId.Value}/transactions", request, JsonOptions);

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
	}
}
