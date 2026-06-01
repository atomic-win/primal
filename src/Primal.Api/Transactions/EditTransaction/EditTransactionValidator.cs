using FastEndpoints;
using FluentValidation;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Api.Transactions;

internal sealed class EditTransactionValidator : Validator<EditTransactionRequest>
{
	private readonly IAssetItemRepository assetItemRepository;
	private readonly IAssetRepository assetRepository;

	public EditTransactionValidator(
		IAssetItemRepository assetItemRepository,
		IAssetRepository assetRepository)
	{
		this.assetItemRepository = assetItemRepository;
		this.assetRepository = assetRepository;

		this.ConfigureAssetItemRules();
		this.ConfigureFieldRules();
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
		this.RuleFor(x => x.Name)
			.MinimumLength(3)
			.When(x => !string.IsNullOrWhiteSpace(x.Name))
			.WithMessage("Transaction name must be at least 3 characters long")
			.WithErrorCode("NAME_TOO_SHORT");

		this.RuleFor(x => x.Name)
			.MaximumLength(1000)
			.When(x => !string.IsNullOrWhiteSpace(x.Name))
			.WithMessage("Transaction name must not exceed 1000 characters")
			.WithErrorCode("NAME_TOO_LONG");

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
			.GreaterThanOrEqualTo(0)
			.When(x => TransactionValidationExtensions.IsUnitsRequired(x.TransactionType))
			.WithMessage("Transaction units must be greater than or equal to zero")
			.WithErrorCode("UNITS_INVALID");

		this.RuleFor(x => x.Price)
			.GreaterThanOrEqualTo(0)
			.When(x => TransactionValidationExtensions.IsUnitsRequired(x.TransactionType))
			.WithMessage("Transaction price must be greater than or equal to zero")
			.WithErrorCode("PRICE_INVALID");

		this.RuleFor(x => x.Amount)
			.GreaterThanOrEqualTo(0)
			.When(x => !TransactionValidationExtensions.IsUnitsRequired(x.TransactionType) && x.TransactionType != TransactionType.Unknown)
			.WithMessage("Transaction amount must be greater than or equal to zero")
			.WithErrorCode("AMOUNT_INVALID");
	}
}
