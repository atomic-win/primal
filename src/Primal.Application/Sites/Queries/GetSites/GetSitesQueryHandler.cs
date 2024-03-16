using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;

namespace Primal.Application.Sites;

internal sealed class GetSitesQueryHandler : IRequestHandler<GetSitesQuery, ErrorOr<IEnumerable<SiteResult>>>
{
	private readonly ISiteRepository siteRepository;

	public GetSitesQueryHandler(ISiteRepository siteRepository)
	{
		this.siteRepository = siteRepository;
	}

	public async Task<ErrorOr<IEnumerable<SiteResult>>> Handle(GetSitesQuery request, CancellationToken cancellationToken)
	{
		var errorOrSites = await this.siteRepository.GetSites(request.UserId, cancellationToken);

		return errorOrSites.Match(
			sites => sites.Select(site => new SiteResult(site.Id, site.Url, site.DailyLimitInMinutes)).ToArray(),
			errors => (ErrorOr<IEnumerable<SiteResult>>)errors);
	}
}
