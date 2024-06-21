using FluentValidation;
using Primal.Domain.Money;

namespace Primal.Application.Investments;

internal sealed class GetPortfolioPerAssetQueryValidator : AbstractValidator<GetPortfolioPerAssetQuery>
{
	public GetPortfolioPerAssetQueryValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
		this.RuleFor(x => x.Currency).IsInEnum().NotEqual(Currency.Unknown);
	}
}
