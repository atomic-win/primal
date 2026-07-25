using FastEndpoints;
using FluentValidation;

using InvestmentPortfolioTracker.Api.Errors;

namespace InvestmentPortfolioTracker.Api.AssetItems;

internal sealed class GetAssetItemValidator : Validator<GetAssetItemRequest>
{
	public GetAssetItemValidator()
	{
		this.RuleFor(x => x.Id)
			.NotEqual(Guid.Empty)
			.WithMessage(ErrorMessages.AssetItem.IdRequired)
			.WithErrorCode(ErrorCodes.AssetItem.IdRequired);
	}
}
