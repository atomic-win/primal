using FastEndpoints;
using FluentValidation;

using InvestmentPortfolioTracker.Api.Errors;
using InvestmentPortfolioTracker.Domain.Money;

namespace InvestmentPortfolioTracker.Api.Transactions;

internal sealed class GetAllByAssetItemIdValidator : Validator<GetAllByAssetItemIdRequest>
{
	public GetAllByAssetItemIdValidator()
	{
		this.RuleFor(x => x.AssetItemId)
			.NotEqual(Guid.Empty)
			.WithMessage(ErrorMessages.AssetItem.IdRequired)
			.WithErrorCode(ErrorCodes.AssetItem.IdRequired);

		this.RuleFor(x => x.Currency)
			.NotEqual(Currency.Unknown)
			.WithMessage(ErrorMessages.AssetItem.CurrencyRequired)
			.WithErrorCode(ErrorCodes.AssetItem.CurrencyRequired);
	}
}
