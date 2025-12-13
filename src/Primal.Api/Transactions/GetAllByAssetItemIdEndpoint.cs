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

	public GetAllByAssetItemIdEndpoint(
		ITransactionRepository transactionRepository,
		IAssetItemRepository assetItemRepository)
	{
		this.transactionRepository = transactionRepository;
		this.assetItemRepository = assetItemRepository;
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

		await this.Send.OkAsync(transactions.ToAsyncEnumerable().Select(t => t.ToResponse()));
	}
}
