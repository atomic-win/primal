using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Sites;

namespace Primal.Application.Sites;

internal sealed class GetSiteByUrlQueryHandler : IRequestHandler<GetSiteByUrlQuery, ErrorOr<SiteResult>>
{
	private readonly ISiteRepository siteRepository;

	public GetSiteByUrlQueryHandler(ISiteRepository siteRepository)
	{
		this.siteRepository = siteRepository;
	}

	public async Task<ErrorOr<SiteResult>> Handle(GetSiteByUrlQuery request, CancellationToken cancellationToken)
	{
		if (!request.Url.IsAllowedUriScheme())
		{
			return request.Url.ToUnallowedSiteResult();
		}

		var errorOrSite = await this.siteRepository.GetSiteByUrl(request.UserId, request.Url, cancellationToken);

		return errorOrSite.Match(
			site => new SiteResult(site.Id, site.Url, site.DailyLimitInMinutes),
			errors => (ErrorOr<SiteResult>)errors);
	}
}
