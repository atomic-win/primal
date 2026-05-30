using System.Net;
using System.Net.Http.Headers;
using NSubstitute;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Api.IntegrationTests.Transactions;

public sealed class DeleteTransactionEndpointTests
{
	[Test]
	public async Task DeleteTransaction_Unauthenticated_Returns401()
	{
		await using var factory = new PrimalApiFactory();
		var client = factory.CreateClient();

		var response = await client.DeleteAsync($"/api/asset-items/{Guid.NewGuid()}/transactions/{Guid.NewGuid()}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
	}

	[Test]
	public async Task DeleteTransaction_NotFound_Returns404()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());
		var transactionId = new TransactionId(Guid.NewGuid());

		factory.TransactionRepository
			.GetByIdAsync(userId, assetItemId, transactionId, Arg.Any<CancellationToken>())
			.Returns(Transaction.Empty);

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.DeleteAsync($"/api/asset-items/{assetItemId.Value}/transactions/{transactionId.Value}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
		await factory.TransactionRepository.DidNotReceive()
			.DeleteAsync(userId, assetItemId, transactionId, Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task DeleteTransaction_Exists_Returns204()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());
		var transactionId = new TransactionId(Guid.NewGuid());
		var transaction = new Transaction(transactionId, new DateOnly(2024, 6, 15), "Deposit", TransactionType.Deposit, assetItemId, 0m, 0m, 100m);

		factory.TransactionRepository
			.GetByIdAsync(userId, assetItemId, transactionId, Arg.Any<CancellationToken>())
			.Returns(transaction);

		factory.TransactionRepository
			.DeleteAsync(userId, assetItemId, transactionId, Arg.Any<CancellationToken>())
			.Returns(Task.CompletedTask);

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.DeleteAsync($"/api/asset-items/{assetItemId.Value}/transactions/{transactionId.Value}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
		await factory.TransactionRepository.Received(1)
			.DeleteAsync(userId, assetItemId, transactionId, Arg.Any<CancellationToken>());
	}
}
