using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;

namespace Primal.Application.Sites;

internal sealed class GetSiteByIdQueryHandler : IRequestHandler<GetSiteByIdQuery, ErrorOr<SiteResult>>
{
	private readonly ISiteRepository siteRepository;

	public GetSiteByIdQueryHandler(ISiteRepository siteRepository)
	{
		this.siteRepository = siteRepository;
	}

	public async Task<ErrorOr<SiteResult>> Handle(GetSiteByIdQuery request, CancellationToken cancellationToken)
	{
		var errorOrSite = await this.siteRepository.GetSiteById(request.UserId, request.SiteId, cancellationToken);

		return errorOrSite.Match(
			site => new SiteResult(site.Id, site.Url, site.DailyLimitInMinutes),
			errors => (ErrorOr<SiteResult>)errors);
	}
}
