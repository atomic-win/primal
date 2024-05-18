using FluentValidation;

namespace Primal.Application.Investments;

internal sealed class GetInstrumentByIdQueryValidator : AbstractValidator<GetInstrumentByIdQuery>
{
	public GetInstrumentByIdQueryValidator()
	{
		this.RuleFor(x => x.InstrumentId.Value).NotEmpty();
	}
}
