using System.Runtime.CompilerServices;
using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Api.Transactions;

[HttpGet("/api/assetItems/{assetItemId:guid}/transactions")]
internal sealed class GetAllByAssetItemIdEndpoint : EndpointWithoutRequest<IAsyncEnumerable<TransactionResponse>>
{
	private readonly ITransactionRepository transactionRepository;
	private readonly IAssetItemRepository assetItemRepository;

	private readonly TransactionAmountCalculator transactionAmountCalculator;

	public GetAllByAssetItemIdEndpoint(
		ITransactionRepository transactionRepository,
		IAssetItemRepository assetItemRepository,
		TransactionAmountCalculator transactionAmountCalculator)
	{
		this.transactionRepository = transactionRepository;
		this.assetItemRepository = assetItemRepository;
		this.transactionAmountCalculator = transactionAmountCalculator;
	}

	public override async Task HandleAsync(
		CancellationToken cancellationToken)
	{
		Guid assetItemId = this.Route<Guid>("assetItemId");
		Currency currency = this.Query<Currency>("currency");

		var assetItem = await this.assetItemRepository.GetByIdAsync(
			this.GetUserId(),
			new AssetItemId(assetItemId),
			cancellationToken);

		if (assetItem.Id == AssetItemId.Empty)
		{
			await this.Send.NotFoundAsync(cancellationToken);
			return;
		}

		var transactions = await this.transactionRepository.GetByAssetItemIdAsync(
			this.GetUserId(),
			new AssetItemId(assetItemId),
			cancellationToken);

		await this.Send.OkAsync(this.MapToResponses(transactions, currency, cancellationToken), cancellationToken);
	}

	private async IAsyncEnumerable<TransactionResponse> MapToResponses(
		IEnumerable<Transaction> transactions,
		Currency currency,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		foreach (var transaction in transactions)
		{
			yield return await transaction.ToResponse(
				this.GetUserId(),
				this.transactionAmountCalculator,
				currency,
				cancellationToken);
		}
	}
}
