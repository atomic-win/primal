using FluentValidation;
using Primal.Domain.Money;

namespace Primal.Application.Investments;

internal sealed class GetExchangeRateQueryValidator : AbstractValidator<GetExchangeRateQuery>
{
	public GetExchangeRateQueryValidator()
	{
		this.RuleFor(x => x.From).IsInEnum().NotEqual(Currency.Unknown);
		this.RuleFor(x => x.To).IsInEnum().NotEqual(Currency.Unknown);
	}
}
