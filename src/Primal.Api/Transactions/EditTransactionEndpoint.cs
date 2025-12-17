using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;

namespace Primal.Api.Transactions;

[HttpPatch("/api/assetItems/{assetItemId:guid}/transactions/{transactionId:guid}")]
internal sealed class EditTransactionEndpoint : Endpoint<EditTransactionRequest>
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
		EditTransactionRequest req,
		CancellationToken cancellationToken)
	{
		this.ValidateRequest(req);

		var existingTransaction = await this.transactionRepository.GetByIdAsync(
			this.GetUserId(),
			new AssetItemId(req.AssetItemId),
			new TransactionId(req.TransactionId),
			cancellationToken);

		if (existingTransaction.Id == TransactionId.Empty)
		{
			this.ThrowError("Transaction does not exist.", StatusCodes.Status404NotFound);
		}

		var assetItem = await this.assetItemRepository.GetByIdAsync(
			this.GetUserId(),
			new AssetItemId(req.AssetItemId),
			cancellationToken);

		if (assetItem.Id == AssetItemId.Empty)
		{
			this.ThrowError("Asset item does not exist.", StatusCodes.Status404NotFound);
		}

		var asset = await this.assetRepository.GetByIdAsync(
			assetItem.AssetId,
			cancellationToken);

		if (req.TransactionType != TransactionType.Unknown
			&& !req.TransactionType.IsValidForAssetType(asset.AssetType))
		{
			this.ThrowError(
				$"Transaction type '{req.TransactionType}' is not valid for asset type '{asset.AssetType}'.",
				StatusCodes.Status400BadRequest);
		}

		await this.transactionRepository.UpdateAsync(
			this.GetUserId(),
			new Transaction(
				existingTransaction.Id,
				existingTransaction.Date,
				name: string.IsNullOrWhiteSpace(req.Name) ? existingTransaction.Name : req.Name,
				transactionType: req.TransactionType == TransactionType.Unknown ? existingTransaction.TransactionType : req.TransactionType,
				assetItemId: new AssetItemId(req.AssetItemId),
				units: req.Units == 0 ? existingTransaction.Units : req.Units),
			cancellationToken);

		await this.Send.NoContentAsync(cancellation: cancellationToken);
	}

	private void ValidateRequest(EditTransactionRequest req)
	{
		if (req.Units < 0)
		{
			this.AddError("Transaction units must be greater than zero.");
		}

		if (string.IsNullOrWhiteSpace(req.Name)
			&& req.TransactionType == TransactionType.Unknown
			&& req.Units == 0)
		{
			this.AddError("At least one of name, transaction type, or units must be provided to update the transaction.");
		}

		if (!string.IsNullOrWhiteSpace(req.Name))
		{
			if (req.Name.Length < 3)
			{
				this.AddError("Transaction name must be at least 3 characters long.");
			}

			if (req.Name.Length > 1000)
			{
				this.AddError("Transaction name must not exceed 1000 characters.");
			}
		}

		this.ThrowIfAnyErrors(StatusCodes.Status400BadRequest);
	}
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0048:File name must match type name", Justification = "Record used only by this endpoint.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Record used only by this endpoint.")]
internal sealed record EditTransactionRequest(
	Guid TransactionId,
	string Name,
	TransactionType TransactionType,
	Guid AssetItemId,
	decimal Units);
