using FastEndpoints;
using Primal.Api.Errors;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Api.Transactions;

[HttpDelete("/api/asset-items/{assetItemId:guid}/transactions/{transactionId:guid}")]
internal sealed class DeleteTransactionEndpoint : Endpoint<DeleteTransactionRequest>
{
	private readonly ITransactionRepository transactionRepository;

	public DeleteTransactionEndpoint(ITransactionRepository transactionRepository)
	{
		this.transactionRepository = transactionRepository;
	}

	public override async Task HandleAsync(DeleteTransactionRequest req, CancellationToken cancellationToken)
	{
		var userId = new UserId(req.UserId);
		var assetItemId = new AssetItemId(req.AssetItemId);
		var transactionId = new TransactionId(req.TransactionId);

		var transaction = await this.transactionRepository.GetByIdAsync(
			userId,
			assetItemId,
			transactionId,
			cancellationToken);

		if (transaction.Id == TransactionId.Empty)
		{
			this.ThrowError(ErrorFactory.TransactionNotFound(), StatusCodes.Status404NotFound);
		}

		await this.transactionRepository.DeleteAsync(
			userId,
			assetItemId,
			transactionId,
			cancellationToken);

		await this.Send.NoContentAsync(cancellation: cancellationToken);
	}
}
