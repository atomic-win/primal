using FluentValidation;
using Primal.Domain.Money;

namespace Primal.Application.Investments;

internal sealed class GetExchangeRatesQueryValidator : AbstractValidator<GetExchangeRatesQuery>
{
	public GetExchangeRatesQueryValidator()
	{
		this.RuleFor(x => x.From).IsInEnum().NotEqual(Currency.Unknown);
		this.RuleFor(x => x.To).IsInEnum().NotEqual(Currency.Unknown);
		this.RuleFor(x => x.StartDate).GreaterThan(DateOnly.MinValue);
		this.RuleFor(x => x.EndDate).LessThan(DateOnly.MaxValue).GreaterThanOrEqualTo(x => x.StartDate);
	}
}
