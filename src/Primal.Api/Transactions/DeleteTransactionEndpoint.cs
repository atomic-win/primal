using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;

namespace Primal.Api.Transactions;

[HttpDelete("/api/assetItems/{assetItemId:guid}/transactions/{transactionId:guid}")]
internal sealed class DeleteTransactionEndpoint : EndpointWithoutRequest
{
	private readonly ITransactionRepository transactionRepository;

	public DeleteTransactionEndpoint(ITransactionRepository transactionRepository)
	{
		this.transactionRepository = transactionRepository;
	}

	public override async Task HandleAsync(CancellationToken cancellationToken)
	{
		Guid assetItemId = this.Route<Guid>("assetItemId");
		Guid transactionId = this.Route<Guid>("transactionId");

		var transaction = await this.transactionRepository.GetByIdAsync(
			this.GetUserId(),
			new AssetItemId(assetItemId),
			new TransactionId(transactionId),
			cancellationToken);

		if (transaction.Id == TransactionId.Empty)
		{
			this.ThrowError("Transaction does not exist.", StatusCodes.Status404NotFound);
		}

		await this.transactionRepository.DeleteAsync(
			this.GetUserId(),
			new AssetItemId(assetItemId),
			new TransactionId(transactionId),
			cancellationToken);

		await this.Send.NoContentAsync(cancellation: cancellationToken);
	}
}
