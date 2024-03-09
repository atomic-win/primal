using FluentValidation;

namespace Primal.Application.Sites;

internal sealed class GetSiteByUrlQueryValidator : AbstractValidator<GetSiteByUrlQuery>
{
	public GetSiteByUrlQueryValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
		this.RuleFor(x => x.Url).NotEmpty();
	}
}
