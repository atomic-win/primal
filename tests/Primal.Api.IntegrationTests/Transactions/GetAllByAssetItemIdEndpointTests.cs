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

public sealed class GetAllByAssetItemIdEndpointTests
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() },
	};

	[Test]
	public async Task GetAllTransactions_Unauthenticated_Returns401()
	{
		await using var factory = new PrimalApiFactory();
		var client = factory.CreateClient();

		var response = await client.GetAsync($"/api/asset-items/{Guid.NewGuid()}/transactions?currency=USD");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
	}

	[Test]
	public async Task GetAllTransactions_AssetItemNotFound_Returns404()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());

		factory.AssetItemRepository
			.GetByIdAsync(userId, assetItemId, Arg.Any<CancellationToken>())
			.Returns(AssetItem.Empty);

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.GetAsync($"/api/asset-items/{assetItemId.Value}/transactions?currency=USD");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}

	[Test]
	public async Task GetAllTransactions_Success_ReturnsTransactions()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var assetId = new AssetId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());
		var assetItem = new AssetItem(assetItemId, assetId, "Brokerage");
		var firstTransaction = new Transaction(new TransactionId(Guid.NewGuid()), new DateOnly(2024, 6, 15), "Buy 1", TransactionType.Buy, assetItemId, 5m, 20m, 0m);
		var secondTransaction = new Transaction(new TransactionId(Guid.NewGuid()), new DateOnly(2024, 6, 16), "Dividend", TransactionType.Dividend, assetItemId, 0m, 0m, 15m);

		factory.AssetItemRepository
			.GetByIdAsync(userId, assetItemId, Arg.Any<CancellationToken>())
			.Returns(assetItem);

		factory.TransactionRepository
			.GetByAssetItemIdAsync(userId, assetItemId, Arg.Any<CancellationToken>())
			.Returns(new[] { firstTransaction, secondTransaction });

		factory.TransactionAmountCalculator
			.CalculateAmountAsync(userId, firstTransaction, firstTransaction.Date, Currency.USD, Arg.Any<CancellationToken>())
			.Returns(100m);

		factory.TransactionAmountCalculator
			.CalculateAmountAsync(userId, secondTransaction, secondTransaction.Date, Currency.USD, Arg.Any<CancellationToken>())
			.Returns(15m);

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.GetAsync($"/api/asset-items/{assetItemId.Value}/transactions?currency=USD");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadFromJsonAsync<TransactionResponse[]>(JsonOptions);
		await Assert.That(body!.Length).IsEqualTo(2);
		await Assert.That(body[0].Id).IsEqualTo(firstTransaction.Id.Value);
		await Assert.That(body[0].Name).IsEqualTo("Buy 1");
		await Assert.That(body[0].Amount).IsEqualTo(100m);
		await Assert.That(body[1].Id).IsEqualTo(secondTransaction.Id.Value);
		await Assert.That(body[1].Name).IsEqualTo("Dividend");
		await Assert.That(body[1].Amount).IsEqualTo(15m);
	}
}
