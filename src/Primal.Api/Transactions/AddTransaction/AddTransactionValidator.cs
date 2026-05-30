using FastEndpoints;
using FluentValidation;
using Primal.Application.Investments;
using Primal.Domain.Investments;

namespace Primal.Api.Transactions;

internal sealed class AddTransactionValidator : Validator<AddTransactionRequest>
{
	private readonly IAssetItemRepository assetItemRepository;
	private readonly IAssetRepository assetRepository;
	private readonly IHttpContextAccessor httpContextAccessor;

	public AddTransactionValidator(
		IAssetItemRepository assetItemRepository,
		IAssetRepository assetRepository,
		IHttpContextAccessor httpContextAccessor)
	{
		this.assetItemRepository = assetItemRepository;
		this.assetRepository = assetRepository;
		this.httpContextAccessor = httpContextAccessor;

		this.ConfigureAssetItemRules();
		this.ConfigureFieldRules();
		this.ConfigureTransactionTypeRules();
		this.ConfigureAmountRules();
	}

	private void ConfigureAssetItemRules()
	{
		this.RuleFor(x => x.AssetItemId)
			.NotEqual(Guid.Empty)
			.WithMessage("Asset item ID must be provided.");

		this.RuleFor(x => x.AssetItemId)
			.MustAsync(async (assetItemId, ct) =>
			{
				var userId = this.httpContextAccessor.GetUserId();
				var assetItem = await this.assetItemRepository.GetByIdAsync(userId, new AssetItemId(assetItemId), ct);
				return assetItem.Id != AssetItemId.Empty;
			})
			.When(x => x.AssetItemId != Guid.Empty)
			.WithMessage("Asset item does not exist.");
	}

	private void ConfigureFieldRules()
	{
		this.RuleFor(x => x.Date)
			.NotEqual(default(DateOnly))
			.WithMessage("Transaction date must be provided.");

		this.RuleFor(x => x.Date)
			.Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow))
			.When(x => x.Date != default)
			.WithMessage("Transaction date cannot be in the future.");

		this.RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage("Transaction name must be provided.");

		this.RuleFor(x => x.Name)
			.MinimumLength(3)
			.WithMessage("Transaction name must be at least 3 characters long.");

		this.RuleFor(x => x.Name)
			.MaximumLength(1000)
			.WithMessage("Transaction name must not exceed 1000 characters.");
	}

	private void ConfigureTransactionTypeRules()
	{
		this.RuleFor(x => x.TransactionType)
			.NotEqual(TransactionType.Unknown)
			.WithMessage("Transaction type must be provided.");

		this.RuleFor(x => x)
			.MustAsync(async (req, ct) =>
			{
				var userId = this.httpContextAccessor.GetUserId();
				var assetItem = await this.assetItemRepository.GetByIdAsync(userId, new AssetItemId(req.AssetItemId), ct);
				if (assetItem.Id == AssetItemId.Empty)
				{
					return true;
				}

				var asset = await this.assetRepository.GetByIdAsync(assetItem.AssetId, ct);
				return TransactionValidationExtensions.IsValidForAssetType(req.TransactionType, asset);
			})
			.When(x => x.AssetItemId != Guid.Empty && x.TransactionType != TransactionType.Unknown)
			.WithMessage(req => $"Transaction type '{req.TransactionType}' is not valid for the asset type.");
	}

	private void ConfigureAmountRules()
	{
		this.RuleFor(x => x.Units)
			.GreaterThan(0)
			.When(x => TransactionValidationExtensions.IsUnitsRequired(x.TransactionType))
			.WithMessage("Transaction units must be greater than zero.");

		this.RuleFor(x => x.Price)
			.GreaterThan(0)
			.When(x => TransactionValidationExtensions.IsUnitsRequired(x.TransactionType))
			.WithMessage("Transaction price must be greater than zero.");

		this.RuleFor(x => x.Amount)
			.GreaterThan(0)
			.When(x => !TransactionValidationExtensions.IsUnitsRequired(x.TransactionType) && x.TransactionType != TransactionType.Unknown)
			.WithMessage("Transaction amount must be greater than zero.");
	}
}
