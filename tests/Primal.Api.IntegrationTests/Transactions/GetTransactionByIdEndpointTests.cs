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

public sealed class GetTransactionByIdEndpointTests
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() },
	};

	[Test]
	public async Task GetTransactionById_Unauthenticated_Returns401()
	{
		await using var factory = new PrimalApiFactory();
		var client = factory.CreateClient();

		var response = await client.GetAsync($"/api/asset-items/{Guid.NewGuid()}/transactions/{Guid.NewGuid()}?currency=USD");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
	}

	[Test]
	public async Task GetTransactionById_Success_ReturnsTransaction()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());
		var transactionId = new TransactionId(Guid.NewGuid());
		var transaction = new Transaction(transactionId, new DateOnly(2024, 6, 15), "Dividend", TransactionType.Dividend, assetItemId, 0m, 0m, 120m);

		factory.TransactionRepository
			.GetByIdAsync(userId, assetItemId, transactionId, Arg.Any<CancellationToken>())
			.Returns(transaction);

		factory.TransactionAmountCalculator
			.CalculateAmountAsync(userId, transaction, transaction.Date, Currency.USD, Arg.Any<CancellationToken>())
			.Returns(120m);

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.GetAsync($"/api/asset-items/{assetItemId.Value}/transactions/{transactionId.Value}?currency=USD");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadFromJsonAsync<TransactionResponse>(JsonOptions);
		await Assert.That(body!.Id).IsEqualTo(transactionId.Value);
		await Assert.That(body.Date).IsEqualTo(transaction.Date);
		await Assert.That(body.Name).IsEqualTo("Dividend");
		await Assert.That(body.TransactionType).IsEqualTo(TransactionType.Dividend);
		await Assert.That(body.AssetItemId).IsEqualTo(assetItemId.Value);
		await Assert.That(body.Units).IsEqualTo(0m);
		await Assert.That(body.Price).IsEqualTo(0m);
		await Assert.That(body.Amount).IsEqualTo(120m);
	}
}
