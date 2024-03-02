using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Application.Sites.Common;

namespace Primal.Application.Sites.Commands;

internal sealed class AddSiteCommandHandler : IRequestHandler<AddSiteCommand, ErrorOr<SiteResult>>
{
	private readonly ISiteRepository siteRepository;

	public AddSiteCommandHandler(ISiteRepository sitesRepository)
	{
		this.siteRepository = sitesRepository;
	}

	public async Task<ErrorOr<SiteResult>> Handle(AddSiteCommand request, CancellationToken cancellationToken)
	{
		var errorOrSite = await this.siteRepository.AddSite(request.UserId, request.Host, request.DailyLimitInMinutes, cancellationToken);

		return errorOrSite.Match(
			site => new SiteResult(site.Id, site.Host, site.DailyLimitInMinutes),
			errors => (ErrorOr<SiteResult>)errors);
	}
}
