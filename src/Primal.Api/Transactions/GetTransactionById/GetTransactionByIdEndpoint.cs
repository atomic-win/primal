using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Api.Transactions;

[HttpGet("/api/asset-items/{assetItemId:guid}/transactions/{transactionId:guid}")]
internal sealed class GetTransactionByIdEndpoint : Endpoint<GetTransactionByIdRequest, TransactionResponse>
{
	private readonly ITransactionRepository transactionRepository;

	private readonly ITransactionAmountCalculator transactionAmountCalculator;

	public GetTransactionByIdEndpoint(
		ITransactionRepository transactionRepository,
		ITransactionAmountCalculator transactionAmountCalculator)
	{
		this.transactionRepository = transactionRepository;
		this.transactionAmountCalculator = transactionAmountCalculator;
	}

	public override async Task HandleAsync(
		GetTransactionByIdRequest req,
		CancellationToken cancellationToken)
	{
		var userId = new UserId(req.UserId);

		var transaction = await this.transactionRepository.GetByIdAsync(
			userId,
			new AssetItemId(req.AssetItemId),
			new TransactionId(req.TransactionId),
			cancellationToken);

		if (transaction.Id == TransactionId.Empty)
		{
			this.ThrowError("Transaction not found", StatusCodes.Status404NotFound);
		}

		var response = await transaction.ToResponse(
			userId,
			this.transactionAmountCalculator,
			req.Currency,
			cancellationToken);

		await this.Send.OkAsync(response, cancellationToken);
	}
}
