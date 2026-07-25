using FastEndpoints;
using FluentValidation;
using InvestmentPortfolioTracker.Api.Errors;
using InvestmentPortfolioTracker.Core.Investments;
using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Api.Transactions;

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
			.WithMessage(ErrorMessages.AssetItem.IdRequired)
			.WithErrorCode(ErrorCodes.AssetItem.IdRequired);
	}

	private void ConfigureFieldRules()
	{
		this.RuleFor(x => x.Date)
			.NotEqual(default(DateOnly))
			.WithMessage(ErrorMessages.Transaction.DateRequired)
			.WithErrorCode(ErrorCodes.Transaction.DateRequired);

		this.RuleFor(x => x.Date)
			.Must(date => date <= DateOnly.FromDateTime(this.timeProvider.GetUtcNow().UtcDateTime))
			.When(x => x.Date != default)
			.WithMessage(ErrorMessages.Transaction.DateInFuture)
			.WithErrorCode(ErrorCodes.Transaction.DateInFuture);

		this.RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage(ErrorMessages.Transaction.NameRequired)
			.WithErrorCode(ErrorCodes.Transaction.NameRequired);

		this.RuleFor(x => x.Name)
			.MinimumLength(3)
			.WithMessage(ErrorMessages.Transaction.NameTooShort)
			.WithErrorCode(ErrorCodes.Transaction.NameTooShort);

		this.RuleFor(x => x.Name)
			.MaximumLength(1000)
			.WithMessage(ErrorMessages.Transaction.NameTooLong)
			.WithErrorCode(ErrorCodes.Transaction.NameTooLong);
	}

	private void ConfigureTransactionTypeRules()
	{
		this.RuleFor(x => x.TransactionType)
			.NotEqual(TransactionType.Unknown)
			.WithMessage(ErrorMessages.Transaction.TypeRequired)
			.WithErrorCode(ErrorCodes.Transaction.TypeRequired);

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
			.GreaterThan(0)
			.When(x => TransactionValidationExtensions.IsUnitsRequired(x.TransactionType))
			.WithMessage(ErrorMessages.Transaction.UnitsRequired)
			.WithErrorCode(ErrorCodes.Transaction.UnitsRequired);

		this.RuleFor(x => x.Price)
			.GreaterThan(0)
			.When(x => TransactionValidationExtensions.IsUnitsRequired(x.TransactionType))
			.WithMessage(ErrorMessages.Transaction.PriceRequired)
			.WithErrorCode(ErrorCodes.Transaction.PriceRequired);

		this.RuleFor(x => x.Amount)
			.GreaterThan(0)
			.When(x => !TransactionValidationExtensions.IsUnitsRequired(x.TransactionType) && x.TransactionType != TransactionType.Unknown)
			.WithMessage(ErrorMessages.Transaction.AmountRequired)
			.WithErrorCode(ErrorCodes.Transaction.AmountRequired);
	}
}
