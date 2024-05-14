using FluentValidation;

namespace Primal.Application.Investments;

internal sealed class GetMutualFundByIdQueryValidator : AbstractValidator<GetMutualFundByIdQuery>
{
	public GetMutualFundByIdQueryValidator()
	{
		this.RuleFor(x => x.Id).NotEmpty();
	}
}
