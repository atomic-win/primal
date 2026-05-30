using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;

namespace Primal.Api.Transactions;

[HttpPatch("/api/asset-items/{assetItemId:guid}/transactions/{transactionId:guid}")]
internal sealed class EditTransactionEndpoint : Endpoint<TransactionRequest>
{
	private readonly ITransactionRepository transactionRepository;
	private readonly IAssetItemRepository assetItemRepository;

	public EditTransactionEndpoint(
		ITransactionRepository transactionRepository,
		IAssetItemRepository assetItemRepository)
	{
		this.transactionRepository = transactionRepository;
		this.assetItemRepository = assetItemRepository;
	}

	public override void Configure()
	{
		this.DontThrowIfValidationFails();
		this.Validator<EditTransactionValidator>();
	}

	public override async Task HandleAsync(
		TransactionRequest req,
		CancellationToken cancellationToken)
	{
		this.ThrowIfAnyErrors(StatusCodes.Status400BadRequest);

		req = await this.NormalizeRequestAsync(req, cancellationToken);

		await this.transactionRepository.UpdateAsync(
			this.GetUserId(),
			new Transaction(
				id: new TransactionId(req.TransactionId),
				req.Date,
				req.Name,
				req.TransactionType,
				assetItemId: new AssetItemId(req.AssetItemId),
				units: req.Units,
				price: req.Price,
				amount: req.Amount),
			cancellationToken);

		await this.Send.NoContentAsync(cancellation: cancellationToken);
	}

	private async Task<TransactionRequest> NormalizeRequestAsync(
		TransactionRequest req,
		CancellationToken cancellationToken)
	{
		var existingTransaction = await this.transactionRepository.GetByIdAsync(
			this.GetUserId(),
			new AssetItemId(req.AssetItemId),
			new TransactionId(req.TransactionId),
			cancellationToken);

		req = req with { Date = existingTransaction.Date };

		if (string.IsNullOrWhiteSpace(req.Name))
		{
			req = req with { Name = existingTransaction.Name };
		}

		if (req.TransactionType == TransactionType.Unknown)
		{
			req = req with { TransactionType = existingTransaction.TransactionType };
		}

		if (!req.IsUnitsRequired())
		{
			return req with { Units = 0, Price = 0 };
		}

		if (req.Units == 0)
		{
			req = req with { Units = existingTransaction.Units };
		}

		if (req.Price == 0)
		{
			req = req with { Price = existingTransaction.Price };
		}

		return req with { Amount = 0 };
	}
}
