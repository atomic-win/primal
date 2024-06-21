using FluentValidation;
using Primal.Domain.Money;

namespace Primal.Application.Investments;

internal sealed class GetPortfolioQueryValidator<T> : AbstractValidator<GetPortfolioQuery<T>>
{
	public GetPortfolioQueryValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
		this.RuleFor(x => x.Currency).IsInEnum().NotEqual(Currency.Unknown);
		this.RuleFor(x => x.IdSelector).NotNull();
	}
}
