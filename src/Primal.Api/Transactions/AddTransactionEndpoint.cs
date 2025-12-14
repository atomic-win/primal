using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;

namespace Primal.Api.Transactions;

[HttpPost("/api/assetItems/{assetItemId:guid}/transactions")]
internal sealed class AddTransactionEndpoint : Endpoint<TransactionRequest>
{
	private readonly ITransactionRepository transactionRepository;
	private readonly IAssetItemRepository assetItemRepository;
	private readonly IAssetRepository assetRepository;

	public AddTransactionEndpoint(
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
		await this.ValidateRequestAsync(
			assetItemId,
			req,
			cancellationToken);

		var transaction = await this.transactionRepository.AddAsync(
			this.GetUserId(),
			new AssetItemId(assetItemId),
			req.Date,
			req.Name,
			req.TransactionType,
			req.Units,
			cancellationToken);

		await this.Send.CreatedAtAsync(
			$"/api/assetItems/{assetItemId}/transactions/{transaction.Id.Value}",
			cancellation: cancellationToken);
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
