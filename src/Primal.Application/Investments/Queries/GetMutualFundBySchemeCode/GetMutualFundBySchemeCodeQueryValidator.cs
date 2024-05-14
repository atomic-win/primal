using FluentValidation;

namespace Primal.Application.Investments;

internal sealed class GetMutualFundBySchemeCodeQueryValidator : AbstractValidator<GetMutualFundBySchemeCodeQuery>
{
	public GetMutualFundBySchemeCodeQueryValidator()
	{
		this.RuleFor(x => x.SchemeCode).GreaterThan(0);
	}
}
