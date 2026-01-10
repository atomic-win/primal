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
		await this.ValidateRequestAsync(
			req,
			cancellationToken);

		var assetItem = await this.assetItemRepository.GetByIdAsync(
			this.GetUserId(),
			new AssetItemId(req.AssetItemId),
			cancellationToken);

		if (assetItem.Id == AssetItemId.Empty)
		{
			this.ThrowError("Asset item does not exist.", StatusCodes.Status404NotFound);
		}

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
			$"/api/assetItems/{req.AssetItemId}/transactions/{transaction.Id.Value}",
			cancellation: cancellationToken);
	}

	private async Task ValidateRequestAsync(
		TransactionRequest req,
		CancellationToken cancellationToken)
	{
		var asset = await this.ValidateAssetItemIdAsync(
			req.AssetItemId,
			cancellationToken);

		this.ValidateDate(req.Date);
		this.ValidateName(req.Name);
		this.ValidateTransactionType(req.TransactionType, asset);

		this.ThrowIfAnyErrors(StatusCodes.Status400BadRequest);

		this.ValidateUnits(req, asset);
		this.ValidatePrice(req, asset);
		this.ValidateAmount(req, asset);

		this.ThrowIfAnyErrors(StatusCodes.Status400BadRequest);
	}

	private async Task<Asset> ValidateAssetItemIdAsync(
		Guid assetItemId,
		CancellationToken cancellationToken)
	{
		if (assetItemId == Guid.Empty)
		{
			this.ThrowError("Asset item ID must be provided.", StatusCodes.Status400BadRequest);
		}

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

		return asset;
	}

	private void ValidateDate(DateOnly date)
	{
		if (date == default)
		{
			this.AddError("Transaction date must be provided.");
		}

		if (date > DateOnly.FromDateTime(DateTime.UtcNow))
		{
			this.AddError("Transaction date cannot be in the future.");
		}
	}

	private void ValidateName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			this.AddError("Transaction name must be provided.");
		}

		if (name.Length < 3)
		{
			this.AddError("Transaction name must be at least 3 characters long.");
		}

		if (name.Length > 1000)
		{
			this.AddError("Transaction name must not exceed 1000 characters.");
		}
	}

	private void ValidateTransactionType(TransactionType transactionType, Asset asset)
	{
		if (transactionType == TransactionType.Unknown)
		{
			this.AddError("Transaction type must be provided.");
			return;
		}

		if (!transactionType.IsValidForAssetType(asset))
		{
			this.AddError(
				$"Transaction type '{transactionType}' is not valid for asset type '{asset.AssetType}'.");
		}
	}

	private void ValidateUnits(TransactionRequest req, Asset asset)
	{
		if (req.Units < 0)
		{
			this.AddError("Transaction units must be greater than zero.");
			return;
		}

		if (req.TransactionType.IsUnitsRequired(asset) && req.Units == 0)
		{
			this.AddError($"Transaction units must be provided for asset type '{asset.AssetType}' and transaction type '{req.TransactionType}'.");
		}

		if (!req.TransactionType.IsUnitsRequired(asset) && req.Units != 0)
		{
			this.AddError($"Transaction units must not be provided for asset type '{asset.AssetType}' and transaction type '{req.TransactionType}'.");
		}
	}

	private void ValidatePrice(TransactionRequest req, Asset asset)
	{
		if (req.Price < 0)
		{
			this.AddError("Transaction price must be greater than zero.");
			return;
		}

		if (req.TransactionType.IsPriceRequired(asset) && req.Price == 0)
		{
			this.AddError($"Transaction price must be provided for asset type '{asset.AssetType}' and transaction type '{req.TransactionType}'.");
		}

		if (!req.TransactionType.IsPriceRequired(asset) && req.Price != 0)
		{
			this.AddError($"Transaction price must not be provided for asset type '{asset.AssetType}' and transaction type '{req.TransactionType}'.");
		}
	}

	private void ValidateAmount(TransactionRequest req, Asset asset)
	{
		if (req.Amount < 0)
		{
			this.AddError("Transaction amount must be greater than zero.");
			return;
		}

		if (req.TransactionType.IsAmountRequired(asset) && req.Amount == 0)
		{
			this.AddError($"Transaction amount must be provided for asset type '{asset.AssetType}' and transaction type '{req.TransactionType}'.");
		}

		if (!req.TransactionType.IsAmountRequired(asset) && req.Amount != 0)
		{
			this.AddError($"Transaction amount must not be provided for asset type '{asset.AssetType}' and transaction type '{req.TransactionType}'.");
		}
	}
}
