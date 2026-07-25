using FastEndpoints;

using InvestmentPortfolioTracker.Api.Errors;
using InvestmentPortfolioTracker.Core.Investments;
using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Api.Transactions;

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
			this.ThrowError(ErrorFactory.TransactionNotFound(), StatusCodes.Status404NotFound);
		}

		var response = await transaction.ToResponse(
			userId,
			this.transactionAmountCalculator,
			req.Currency,
			cancellationToken);

		await this.Send.OkAsync(response, cancellationToken);
	}
}
