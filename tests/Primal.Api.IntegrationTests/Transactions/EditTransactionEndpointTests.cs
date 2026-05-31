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

public sealed class EditTransactionEndpointTests
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() },
	};

	[Test]
	public async Task EditTransaction_Unauthenticated_Returns401()
	{
		await using var factory = new PrimalApiFactory();
		var assetItemId = Guid.NewGuid();
		var transactionId = Guid.NewGuid();
		var client = factory.CreateClient();

		var request = new EditTransactionRequest(assetItemId, transactionId, "Updated buy", TransactionType.Buy, 12m, 11m, 0m);
		var response = await client.PatchAsJsonAsync($"/api/asset-items/{assetItemId}/transactions/{transactionId}", request, JsonOptions);

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
	}

	[Test]
	public async Task EditTransaction_ValidRequest_Returns204()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var assetId = new AssetId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());
		var transactionId = new TransactionId(Guid.NewGuid());
		var assetItem = new AssetItem(assetItemId, assetId, "My MF");
		var asset = new Asset(assetId, "Test MF", AssetClass.Equity, AssetType.MutualFund, Currency.USD, "mf-123456");
		var existingTransaction = new Transaction(transactionId, new DateOnly(2024, 6, 15), "Initial buy", TransactionType.Buy, assetItemId, 10m, 8m, 0m);

		factory.AssetItemRepository
			.GetByIdAsync(userId, assetItemId, Arg.Any<CancellationToken>())
			.Returns(assetItem);

		factory.AssetRepository
			.GetByIdAsync(assetId, Arg.Any<CancellationToken>())
			.Returns(asset);

		factory.TransactionRepository
			.GetByIdAsync(userId, assetItemId, transactionId, Arg.Any<CancellationToken>())
			.Returns(existingTransaction);

		factory.TransactionRepository
			.UpdateAsync(
				userId,
				Arg.Is<Transaction>(transaction =>
					transaction.Id == transactionId &&
					transaction.Date == existingTransaction.Date &&
					transaction.Name == "Updated buy" &&
					transaction.TransactionType == TransactionType.Buy &&
					transaction.AssetItemId == assetItemId &&
					transaction.Units == 12m &&
					transaction.Price == 11m &&
					transaction.Amount == 0m),
				Arg.Any<CancellationToken>())
			.Returns(Task.CompletedTask);

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var request = new EditTransactionRequest(assetItemId.Value, transactionId.Value, "Updated buy", TransactionType.Buy, 12m, 11m, 0m);
		var response = await client.PatchAsJsonAsync($"/api/asset-items/{assetItemId.Value}/transactions/{transactionId.Value}", request, JsonOptions);

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
		await factory.TransactionRepository.Received(1)
			.UpdateAsync(
				userId,
				Arg.Is<Transaction>(transaction =>
					transaction.Id == transactionId &&
					transaction.Date == existingTransaction.Date &&
					transaction.Name == "Updated buy" &&
					transaction.TransactionType == TransactionType.Buy &&
					transaction.AssetItemId == assetItemId &&
					transaction.Units == 12m &&
					transaction.Price == 11m &&
					transaction.Amount == 0m),
				Arg.Any<CancellationToken>());
	}
}
