using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Api.Transactions;

[HttpGet("/api/assetItems/{assetItemId:guid}/transactions?currency={currency}")]
internal sealed class GetAllByAssetItemIdEndpoint : EndpointWithoutRequest<IAsyncEnumerable<TransactionResponse>>
{
	private readonly ITransactionRepository transactionRepository;

	public GetAllByAssetItemIdEndpoint(ITransactionRepository transactionRepository)
	{
		this.transactionRepository = transactionRepository;
	}

	public override async Task<IAsyncEnumerable<TransactionResponse>> ExecuteAsync(
		CancellationToken cancellationToken)
	{
		Guid assetItemId = this.Route<Guid>("assetItemId");
		Currency currency = this.Query<Currency>("currency");

		var transactions = await this.transactionRepository.GetByAssetItemIdAsync(
			this.GetUserId(),
			new AssetItemId(assetItemId),
			cancellationToken);

		return transactions.ToAsyncEnumerable().Select(t => t.ToResponse());
	}
}
