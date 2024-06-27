using FluentValidation;
using Primal.Domain.Money;

namespace Primal.Application.Investments;

internal sealed class GetPortfolioQueryValidator : AbstractValidator<GetPortfolioQuery>
{
	public GetPortfolioQueryValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
		this.RuleFor(x => x.Currency).IsInEnum().NotEqual(Currency.Unknown);
	}
}
