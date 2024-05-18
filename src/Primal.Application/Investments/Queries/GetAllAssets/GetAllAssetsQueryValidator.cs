using FluentValidation;

namespace Primal.Application.Investments;

internal sealed class GetAllAssetsQueryValidator : AbstractValidator<GetAllAssetsQuery>
{
	public GetAllAssetsQueryValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
	}
}
