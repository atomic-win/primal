using FastEndpoints;
using FluentValidation;

using InvestmentPortfolioTracker.Api.Errors;

namespace InvestmentPortfolioTracker.Api.AssetItems;

internal sealed class EditAssetItemValidator : Validator<EditAssetItemRequest>
{
	public EditAssetItemValidator()
	{
		this.RuleFor(x => x.Id)
			.NotEqual(Guid.Empty)
			.WithMessage(ErrorMessages.AssetItem.IdRequired)
			.WithErrorCode(ErrorCodes.AssetItem.IdRequired);

		this.RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage(ErrorMessages.AssetItem.NameRequired)
			.WithErrorCode(ErrorCodes.AssetItem.NameRequired);

		this.RuleFor(x => x.Name)
			.MinimumLength(3)
			.When(x => !string.IsNullOrWhiteSpace(x.Name))
			.WithMessage(ErrorMessages.AssetItem.NameTooShort)
			.WithErrorCode(ErrorCodes.AssetItem.NameTooShort);

		this.RuleFor(x => x.Name)
			.MaximumLength(50)
			.When(x => !string.IsNullOrWhiteSpace(x.Name))
			.WithMessage(ErrorMessages.AssetItem.NameTooLong)
			.WithErrorCode(ErrorCodes.AssetItem.NameTooLong);
	}
}
