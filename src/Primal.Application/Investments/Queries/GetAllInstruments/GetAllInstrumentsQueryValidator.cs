using FluentValidation;

namespace Primal.Application.Investments;

internal sealed class GetAllInstrumentsQueryValidator : AbstractValidator<GetAllInstrumentsQuery>
{
	public GetAllInstrumentsQueryValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
	}
}
