using FluentValidation;

namespace Primal.Application.Sites;

internal sealed class GetSitesQueryValidator : AbstractValidator<GetSitesQuery>
{
	public GetSitesQueryValidator()
	{
		this.RuleFor(x => x.UserId).NotEmpty();
	}
}
