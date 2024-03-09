using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;

namespace Primal.Application.Sites;

internal sealed class AddSiteCommandHandler : IRequestHandler<AddSiteCommand, ErrorOr<SiteResult>>
{
	private readonly ISiteRepository siteRepository;

	public AddSiteCommandHandler(ISiteRepository sitesRepository)
	{
		this.siteRepository = sitesRepository;
	}

	public async Task<ErrorOr<SiteResult>> Handle(AddSiteCommand request, CancellationToken cancellationToken)
	{
		var errorOrSite = await this.siteRepository.AddSite(request.UserId, request.Url, request.DailyLimitInMinutes, cancellationToken);

		return errorOrSite.Match(
			site => new SiteResult(site.Id, site.Url, site.DailyLimitInMinutes),
			errors => (ErrorOr<SiteResult>)errors);
	}
}
