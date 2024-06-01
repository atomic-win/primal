using FluentValidation;

namespace Primal.Application.Investments;

internal sealed class GetInstrumentValueQueryValidator : AbstractValidator<GetInstrumentValueQuery>
{
	public GetInstrumentValueQueryValidator()
	{
		this.RuleFor(x => x.InstrumentId.Value).NotEmpty();
		this.RuleFor(x => x.StartDate).GreaterThan(DateOnly.MinValue);
		this.RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).LessThan(DateOnly.MaxValue);
	}
}
