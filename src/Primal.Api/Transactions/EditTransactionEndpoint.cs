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
		req = await this.ValidateRequest(
			req,
			cancellationToken);

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

	private async Task<TransactionRequest> ValidateRequest(
		TransactionRequest req,
		CancellationToken cancellationToken)
	{
		var asset = await this.ValidateAssetItemIdAsync(
			req.AssetItemId,
			cancellationToken);

		var existingTransaction = await this.transactionRepository.GetByIdAsync(
			this.GetUserId(),
			new AssetItemId(req.AssetItemId),
			new TransactionId(req.TransactionId),
			cancellationToken);

		if (existingTransaction.Id == TransactionId.Empty)
		{
			this.ThrowError("Transaction does not exist.", StatusCodes.Status404NotFound);
		}

		req = this.ValidateDate(req, existingTransaction);
		req = this.ValidateName(req, existingTransaction);
		req = this.ValidateTransactionType(req, asset, existingTransaction);
		req = this.ValidateUnits(req, asset, existingTransaction);
		req = this.ValidatePrice(req, asset, existingTransaction);
		req = this.ValidateAmount(req, asset, existingTransaction);

		this.ThrowIfAnyErrors(StatusCodes.Status400BadRequest);

		return req;
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

	private TransactionRequest ValidateDate(TransactionRequest req, Transaction existingTransaction)
	{
		return req with { Date = existingTransaction.Date };
	}

	private TransactionRequest ValidateName(TransactionRequest req, Transaction existingTransaction)
	{
		if (string.IsNullOrWhiteSpace(req.Name))
		{
			return req with { Name = existingTransaction.Name };
		}

		if (req.Name.Length < 3)
		{
			this.AddError("Transaction name must be at least 3 characters long.");
		}

		if (req.Name.Length > 1000)
		{
			this.AddError("Transaction name must not exceed 1000 characters.");
		}

		return req;
	}

	private TransactionRequest ValidateTransactionType(TransactionRequest req, Asset asset, Transaction existingTransaction)
	{
		if (req.TransactionType == TransactionType.Unknown)
		{
			return req with { TransactionType = existingTransaction.TransactionType };
		}

		if (!req.IsValidForAssetType(asset))
		{
			this.AddError(
				$"Transaction type '{req.TransactionType}' is not valid for asset type '{asset.AssetType}'.");
		}

		this.ThrowIfAnyErrors(StatusCodes.Status400BadRequest);
		return req;
	}

	private TransactionRequest ValidateUnits(TransactionRequest req, Asset asset, Transaction existingTransaction)
	{
		if (!req.IsUnitsRequired(asset))
		{
			return req with { Units = 0 };
		}

		if (req.Units < 0)
		{
			this.AddError("Transaction units must be greater than or equal to zero.");
			return req;
		}

		if (req.Units == 0)
		{
			return req with { Units = existingTransaction.Units };
		}

		return req;
	}

	private TransactionRequest ValidatePrice(TransactionRequest req, Asset asset, Transaction existingTransaction)
	{
		if (!req.IsPriceRequired(asset))
		{
			return req with { Price = 0 };
		}

		if (req.Price < 0)
		{
			this.AddError("Transaction price must be greater than or equal to zero.");
			return req;
		}

		if (req.Price == 0)
		{
			return req with { Price = existingTransaction.Price };
		}

		return req;
	}

	private TransactionRequest ValidateAmount(TransactionRequest req, Asset asset, Transaction existingTransaction)
	{
		if (!req.IsAmountRequired(asset))
		{
			return req with { Amount = 0 };
		}

		if (req.Amount < 0)
		{
			this.AddError("Transaction amount must be greater than or equal to zero.");
			return req;
		}

		if (req.Amount == 0)
		{
			return req with { Amount = existingTransaction.Amount };
		}

		return req;
	}
}
