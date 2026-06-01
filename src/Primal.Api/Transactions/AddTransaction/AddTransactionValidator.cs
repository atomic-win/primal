using FastEndpoints;
using FluentValidation;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Api.Transactions;

internal sealed class AddTransactionValidator : Validator<AddTransactionRequest>
{
	private readonly IAssetItemRepository assetItemRepository;
	private readonly IAssetRepository assetRepository;
	private readonly TimeProvider timeProvider;

	public AddTransactionValidator(
		IAssetItemRepository assetItemRepository,
		IAssetRepository assetRepository,
		TimeProvider timeProvider)
	{
		this.assetItemRepository = assetItemRepository;
		this.assetRepository = assetRepository;
		this.timeProvider = timeProvider;

		this.ConfigureAssetItemRules();
		this.ConfigureFieldRules();
		this.ConfigureTransactionTypeRules();
		this.ConfigureAmountRules();
	}

	private void ConfigureAssetItemRules()
	{
		this.RuleFor(x => x.AssetItemId)
			.NotEqual(Guid.Empty)
			.WithMessage("Asset item ID must be provided")
			.WithErrorCode("ASSET_ITEM_ID_REQUIRED");
	}

	private void ConfigureFieldRules()
	{
		this.RuleFor(x => x.Date)
			.NotEqual(default(DateOnly))
			.WithMessage("Transaction date must be provided")
			.WithErrorCode("DATE_REQUIRED");

		this.RuleFor(x => x.Date)
			.Must(date => date <= DateOnly.FromDateTime(this.timeProvider.GetUtcNow().UtcDateTime))
			.When(x => x.Date != default)
			.WithMessage("Transaction date cannot be in the future")
			.WithErrorCode("DATE_IN_FUTURE");

		this.RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage("Transaction name must be provided")
			.WithErrorCode("NAME_REQUIRED");

		this.RuleFor(x => x.Name)
			.MinimumLength(3)
			.WithMessage("Transaction name must be at least 3 characters long")
			.WithErrorCode("NAME_TOO_SHORT");

		this.RuleFor(x => x.Name)
			.MaximumLength(1000)
			.WithMessage("Transaction name must not exceed 1000 characters")
			.WithErrorCode("NAME_TOO_LONG");
	}

	private void ConfigureTransactionTypeRules()
	{
		this.RuleFor(x => x.TransactionType)
			.NotEqual(TransactionType.Unknown)
			.WithMessage("Transaction type must be provided")
			.WithErrorCode("TRANSACTION_TYPE_REQUIRED");

		this.RuleFor(x => x)
			.MustAsync(async (req, ct) =>
			{
				var userId = new UserId(req.UserId);
				var assetItem = await this.assetItemRepository.GetByIdAsync(userId, new AssetItemId(req.AssetItemId), ct);
				if (assetItem.Id == AssetItemId.Empty)
				{
					return true;
				}

				var asset = await this.assetRepository.GetByIdAsync(assetItem.AssetId, ct);
				return TransactionValidationExtensions.IsValidForAssetType(req.TransactionType, asset);
			})
			.When(x => x.AssetItemId != Guid.Empty && x.TransactionType != TransactionType.Unknown)
			.WithMessage(req => $"Transaction type '{req.TransactionType}' is not valid for the asset type")
			.WithErrorCode("TRANSACTION_TYPE_INVALID");
	}

	private void ConfigureAmountRules()
	{
		this.RuleFor(x => x.Units)
			.GreaterThan(0)
			.When(x => TransactionValidationExtensions.IsUnitsRequired(x.TransactionType))
			.WithMessage("Transaction units must be greater than zero")
			.WithErrorCode("UNITS_REQUIRED");

		this.RuleFor(x => x.Price)
			.GreaterThan(0)
			.When(x => TransactionValidationExtensions.IsUnitsRequired(x.TransactionType))
			.WithMessage("Transaction price must be greater than zero")
			.WithErrorCode("PRICE_REQUIRED");

		this.RuleFor(x => x.Amount)
			.GreaterThan(0)
			.When(x => !TransactionValidationExtensions.IsUnitsRequired(x.TransactionType) && x.TransactionType != TransactionType.Unknown)
			.WithMessage("Transaction amount must be greater than zero")
			.WithErrorCode("AMOUNT_REQUIRED");
	}
}
