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
		req = await this.ValidateRequestAsync(
			req,
			cancellationToken);

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

	private async Task<TransactionRequest> ValidateRequestAsync(
		TransactionRequest req,
		CancellationToken cancellationToken)
	{
		var asset = await this.ValidateAssetItemIdAsync(
			req.AssetItemId,
			cancellationToken);

		this.ValidateDate(req);
		this.ValidateName(req);
		this.ValidateTransactionType(req, asset);

		req = this.ValidateUnits(req);
		req = this.ValidatePrice(req);
		req = this.ValidateAmount(req);

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

	private void ValidateDate(TransactionRequest req)
	{
		if (req.Date == default)
		{
			this.AddError("Transaction date must be provided.");
		}

		if (req.Date > DateOnly.FromDateTime(DateTime.UtcNow))
		{
			this.AddError("Transaction date cannot be in the future.");
		}
	}

	private void ValidateName(TransactionRequest req)
	{
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
	}

	private void ValidateTransactionType(TransactionRequest req, Asset asset)
	{
		if (req.TransactionType == TransactionType.Unknown)
		{
			this.AddError("Transaction type must be provided.");
			return;
		}

		if (!req.IsValidForAssetType(asset))
		{
			this.AddError(
				$"Transaction type '{req.TransactionType}' is not valid for asset type '{asset.AssetType}'.");
		}

		this.ThrowIfAnyErrors(StatusCodes.Status400BadRequest);
	}

	private TransactionRequest ValidateUnits(TransactionRequest req)
	{
		if (!req.IsUnitsRequired())
		{
			return req with { Units = 0 };
		}

		if (req.Units <= 0)
		{
			this.AddError("Transaction units must be greater than zero.");
		}

		return req;
	}

	private TransactionRequest ValidatePrice(TransactionRequest req)
	{
		if (!req.IsUnitsRequired())
		{
			return req with { Price = 0 };
		}

		if (req.Price <= 0)
		{
			this.AddError("Transaction price must be greater than zero.");
		}

		return req;
	}

	private TransactionRequest ValidateAmount(TransactionRequest req)
	{
		if (req.IsUnitsRequired())
		{
			return req with { Amount = 0 };
		}

		if (req.Amount <= 0)
		{
			this.AddError("Transaction amount must be greater than zero.");
		}

		return req;
	}
}
