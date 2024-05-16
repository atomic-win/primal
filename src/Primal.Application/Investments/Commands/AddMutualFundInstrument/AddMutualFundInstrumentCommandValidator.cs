using FluentValidation;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class AddMutualFundInstrumentCommandValidator : AbstractValidator<AddMutualFundInstrumentCommand>
{
	public AddMutualFundInstrumentCommandValidator()
	{
		this.RuleFor(x => x.UserId).NotEmpty();
		this.RuleFor(x => x.Name).NotEmpty();
		this.RuleFor(x => x.Category).IsInEnum().Must(x => x == InvestmentCategory.Equity);
		this.RuleFor(x => x.SchemeCode).NotEmpty();
	}
}
