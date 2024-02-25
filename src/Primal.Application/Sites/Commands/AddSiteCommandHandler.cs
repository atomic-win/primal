using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Sites;
using Primal.Application.Sites.Common;

namespace Primal.Application.Sites.Commands;

internal sealed class AddSiteCommandHandler : IRequestHandler<AddSiteCommand, ErrorOr<SiteResult>>
{
	private readonly ISitesRepository sitesRepository;

	public AddSiteCommandHandler(ISitesRepository sitesRepository)
	{
		this.sitesRepository = sitesRepository;
	}

	public async Task<ErrorOr<SiteResult>> Handle(AddSiteCommand request, CancellationToken cancellationToken)
	{
		var errorOrSite = await this.sitesRepository.AddSite(request.UserId, request.Url, request.DailyLimitInMinutes);

		return errorOrSite.Match(
			site => new SiteResult(site.Id, site.Url, site.DailyLimitInMinutes),
			errors => (ErrorOr<SiteResult>)errors);
	}
}
