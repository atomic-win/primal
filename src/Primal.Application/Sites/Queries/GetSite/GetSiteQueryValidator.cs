using FluentValidation;

namespace Primal.Application.Sites;

internal sealed class GetSiteQueryValidator : AbstractValidator<GetSiteQuery>
{
	public GetSiteQueryValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
		this.RuleFor(x => x.SiteId.Value).NotEmpty();
	}
}
