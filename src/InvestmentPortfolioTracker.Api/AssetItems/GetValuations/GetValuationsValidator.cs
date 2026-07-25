using FastEndpoints;
using FluentValidation;

using InvestmentPortfolioTracker.Api.Errors;
using InvestmentPortfolioTracker.Domain.Money;

namespace InvestmentPortfolioTracker.Api.AssetItems;

internal sealed class GetValuationsValidator : Validator<GetValuationsRequest>
{
	public GetValuationsValidator()
	{
		this.RuleFor(x => x.AssetItemIds)
			.NotEmpty()
			.WithMessage(ErrorMessages.AssetItem.IdsRequired)
			.WithErrorCode(ErrorCodes.AssetItem.IdsRequired);

		this.RuleFor(x => x.Currency)
			.NotEqual(Currency.Unknown)
			.WithMessage(ErrorMessages.AssetItem.CurrencyRequired)
			.WithErrorCode(ErrorCodes.AssetItem.CurrencyRequired);
	}
}
