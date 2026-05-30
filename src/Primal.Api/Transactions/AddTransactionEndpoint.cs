using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;

namespace Primal.Api.Transactions;

[HttpPost("/api/asset-items/{assetItemId:guid}/transactions")]
internal sealed class AddTransactionEndpoint : Endpoint<TransactionRequest>
{
	private readonly ITransactionRepository transactionRepository;

	public AddTransactionEndpoint(
		ITransactionRepository transactionRepository)
	{
		this.transactionRepository = transactionRepository;
	}

	public override void Configure()
	{
		this.DontThrowIfValidationFails();
		this.Validator<AddTransactionValidator>();
	}

	public override async Task HandleAsync(
		TransactionRequest req,
		CancellationToken cancellationToken)
	{
		this.ThrowIfAnyErrors(StatusCodes.Status400BadRequest);

		req = this.NormalizeRequest(req);

		var transaction = await this.transactionRepository.AddAsync(
			this.GetUserId(),
			new AssetItemId(req.AssetItemId),
			req.Date,
			req.Name,
			req.TransactionType,
			req.Units,
			req.Price,
			req.Amount,
			cancellationToken);

		await this.Send.CreatedAtAsync(
			$"/api/asset-items/{req.AssetItemId}/transactions/{transaction.Id.Value}",
			cancellation: cancellationToken);
	}

	private TransactionRequest NormalizeRequest(TransactionRequest req)
	{
		if (!req.IsUnitsRequired())
		{
			return req with { Units = 0, Price = 0 };
		}

		return req with { Amount = 0 };
	}
}
