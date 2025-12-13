using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Api.Transactions;

[HttpGet("/api/assetItems/{assetItemId:guid}/transactions/{transactionId:guid}")]
internal sealed class GetTransactionByIdEndpoint : EndpointWithoutRequest<TransactionResponse>
{
	private readonly ITransactionRepository transactionRepository;

	public GetTransactionByIdEndpoint(ITransactionRepository transactionRepository)
	{
		this.transactionRepository = transactionRepository;
	}

	public override async Task<TransactionResponse> ExecuteAsync(
		CancellationToken cancellationToken)
	{
		Guid assetItemId = this.Route<Guid>("assetItemId");
		Guid transactionId = this.Route<Guid>("transactionId");
		Currency currency = this.Query<Currency>("currency");

		var transaction = await this.transactionRepository.GetByIdAsync(
			this.GetUserId(),
			new AssetItemId(assetItemId),
			new TransactionId(transactionId),
			cancellationToken);

		return transaction.ToResponse();
	}
}
