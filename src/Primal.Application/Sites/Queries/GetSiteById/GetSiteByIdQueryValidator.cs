using FluentValidation;

namespace Primal.Application.Sites;

internal sealed class GetSiteByIdQueryValidator : AbstractValidator<GetSiteByIdQuery>
{
	public GetSiteByIdQueryValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
		this.RuleFor(x => x.SiteId.Value).NotEmpty();
	}
}
