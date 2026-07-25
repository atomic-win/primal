using FastEndpoints;
using FluentValidation;
using InvestmentPortfolioTracker.Api.Errors;
using InvestmentPortfolioTracker.Core.Investments;
using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Api.Transactions;

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
			.WithMessage(ErrorMessages.AssetItem.IdRequired)
			.WithErrorCode(ErrorCodes.AssetItem.IdRequired);
	}

	private void ConfigureFieldRules()
	{
		this.RuleFor(x => x.Name)
			.MinimumLength(3)
			.When(x => !string.IsNullOrWhiteSpace(x.Name))
			.WithMessage(ErrorMessages.Transaction.NameTooShort)
			.WithErrorCode(ErrorCodes.Transaction.NameTooShort);

		this.RuleFor(x => x.Name)
			.MaximumLength(1000)
			.When(x => !string.IsNullOrWhiteSpace(x.Name))
			.WithMessage(ErrorMessages.Transaction.NameTooLong)
			.WithErrorCode(ErrorCodes.Transaction.NameTooLong);

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
			.WithErrorCode(ErrorCodes.Transaction.TypeInvalid);
	}

	private void ConfigureAmountRules()
	{
		this.RuleFor(x => x.Units)
			.GreaterThanOrEqualTo(0)
			.When(x => TransactionValidationExtensions.IsUnitsRequired(x.TransactionType))
			.WithMessage(ErrorMessages.Transaction.UnitsInvalid)
			.WithErrorCode(ErrorCodes.Transaction.UnitsInvalid);

		this.RuleFor(x => x.Price)
			.GreaterThanOrEqualTo(0)
			.When(x => TransactionValidationExtensions.IsUnitsRequired(x.TransactionType))
			.WithMessage(ErrorMessages.Transaction.PriceInvalid)
			.WithErrorCode(ErrorCodes.Transaction.PriceInvalid);

		this.RuleFor(x => x.Amount)
			.GreaterThanOrEqualTo(0)
			.When(x => !TransactionValidationExtensions.IsUnitsRequired(x.TransactionType) && x.TransactionType != TransactionType.Unknown)
			.WithMessage(ErrorMessages.Transaction.AmountInvalid)
			.WithErrorCode(ErrorCodes.Transaction.AmountInvalid);
	}
}
