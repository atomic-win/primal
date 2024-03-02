using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;

namespace Primal.Application.Sites;

internal sealed class GetSiteQueryHandler : IRequestHandler<GetSiteQuery, ErrorOr<SiteResult>>
{
	private readonly ISiteRepository siteRepository;

	public GetSiteQueryHandler(ISiteRepository siteRepository)
	{
		this.siteRepository = siteRepository;
	}

	public async Task<ErrorOr<SiteResult>> Handle(GetSiteQuery request, CancellationToken cancellationToken)
	{
		var errorOrSite = await this.siteRepository.GetSite(request.UserId, request.SiteId, cancellationToken);

		return errorOrSite.Match(
			site => new SiteResult(site.Id, site.Host, site.DailyLimitInMinutes),
			errors => (ErrorOr<SiteResult>)errors);
	}
}
