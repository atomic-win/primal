using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;

namespace Primal.Api.Transactions;

[HttpPatch("/api/assetItems/{assetItemId:guid}/transactions/{transactionId:guid}")]
internal sealed class EditTransactionEndpoint : Endpoint<TransactionRequest>
{
	private readonly ITransactionRepository transactionRepository;
	private readonly IAssetItemRepository assetItemRepository;
	private readonly IAssetRepository assetRepository;

	public EditTransactionEndpoint(
		ITransactionRepository transactionRepository,
		IAssetItemRepository assetItemRepository,
		IAssetRepository assetRepository)
	{
		this.transactionRepository = transactionRepository;
		this.assetItemRepository = assetItemRepository;
		this.assetRepository = assetRepository;
	}

	public override async Task HandleAsync(
		TransactionRequest req,
		CancellationToken cancellationToken)
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

		var updatedRequest = this.GetRequestWithDefaults(req, transaction);

		await this.ValidateRequestAsync(
			assetItemId,
			updatedRequest,
			cancellationToken);

		await this.transactionRepository.UpdateAsync(
			this.GetUserId(),
			new Transaction(
				transaction.Id,
				updatedRequest.Date,
				updatedRequest.Name,
				updatedRequest.TransactionType,
				new AssetItemId(assetItemId),
				updatedRequest.Units),
			cancellationToken);

		await this.Send.NoContentAsync(cancellation: cancellationToken);
	}

	private TransactionRequest GetRequestWithDefaults(TransactionRequest req, Transaction existingTransaction)
	{
		return new TransactionRequest(
			Date: req.Date == default ? existingTransaction.Date : req.Date,
			Name: string.IsNullOrWhiteSpace(req.Name) ? existingTransaction.Name : req.Name,
			TransactionType: req.TransactionType == TransactionType.Unknown ? existingTransaction.TransactionType : req.TransactionType,
			AssetItemId: existingTransaction.AssetItemId.Value,
			Units: req.Units <= 0 ? existingTransaction.Units : req.Units);
	}

	private async Task ValidateRequestAsync(
		Guid assetItemId,
		TransactionRequest req,
		CancellationToken cancellationToken)
	{
		if (req.Date == default)
		{
			this.AddError("Transaction date must be provided.");
		}

		if (string.IsNullOrWhiteSpace(req.Name))
		{
			this.AddError("Transaction name must be provided.");
		}

		if (req.Name.Length < 3)
		{
			this.AddError("Transaction name must be at least 3 characters long.");
		}

		if (req.Name.Length > 1000)
		{
			this.AddError("Transaction name must not exceed 1000 characters.");
		}

		if (req.TransactionType == TransactionType.Unknown)
		{
			this.AddError("Transaction type must be provided.");
		}

		if (req.Units <= 0)
		{
			this.AddError("Transaction units must be greater than zero.");
		}

		this.ThrowIfAnyErrors(StatusCodes.Status400BadRequest);

		var assetItem = await this.assetItemRepository.GetByIdAsync(
			this.GetUserId(),
			new AssetItemId(assetItemId),
			cancellationToken);

		if (assetItem.Id == AssetItemId.Empty)
		{
			this.ThrowError("Asset item does not exist.", StatusCodes.Status404NotFound);
		}

		var asset = await this.assetRepository.GetByIdAsync(
			assetItem.AssetId,
			cancellationToken);

		if (!req.TransactionType.IsValidForAssetType(asset.AssetType))
		{
			this.ThrowError(
				$"Transaction type '{req.TransactionType}' is not valid for asset type '{asset.AssetType}'.",
				StatusCodes.Status400BadRequest);
		}
	}
}
