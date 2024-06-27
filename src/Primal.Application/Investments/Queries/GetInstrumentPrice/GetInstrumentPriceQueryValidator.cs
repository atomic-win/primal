using FluentValidation;

namespace Primal.Application.Investments;

internal sealed class GetInstrumentPriceQueryValidator : AbstractValidator<GetInstrumentPriceQuery>
{
	public GetInstrumentPriceQueryValidator()
	{
		this.RuleFor(x => x.InstrumentId.Value).NotEmpty();
	}
}
