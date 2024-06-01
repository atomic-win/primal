using FluentValidation;

namespace Primal.Application.Investments;

internal sealed class GetExchangeRatesQueryValidator : AbstractValidator<GetExchangeRatesQuery>
{
	public GetExchangeRatesQueryValidator()
	{
		this.RuleFor(x => x.From).IsInEnum();
		this.RuleFor(x => x.To).IsInEnum().NotEqual(x => x.From);
		this.RuleFor(x => x.StartDate).GreaterThan(DateOnly.MinValue);
		this.RuleFor(x => x.EndDate).LessThan(DateOnly.MaxValue).GreaterThanOrEqualTo(x => x.StartDate);
	}
}
