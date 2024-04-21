using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Sites;

namespace Primal.Application.Sites;

internal sealed class AddSiteCommandHandler : IRequestHandler<AddSiteCommand, ErrorOr<SiteResult>>
{
	private readonly HashSet<string> allowedUriSchemes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"http",
		"https",
	};

	private readonly ISiteRepository siteRepository;

	public AddSiteCommandHandler(ISiteRepository sitesRepository)
	{
		this.siteRepository = sitesRepository;
	}

	public async Task<ErrorOr<SiteResult>> Handle(AddSiteCommand request, CancellationToken cancellationToken)
	{
		if (!this.allowedUriSchemes.Contains(request.Url.Scheme))
		{
			return new SiteResult(new SiteId(Guid.Empty), request.Url.Host, 0);
		}

		var errorOrSite = await this.siteRepository.AddSite(request.UserId, request.Url, request.DailyLimitInMinutes, cancellationToken);

		return errorOrSite.Match(
			site => new SiteResult(site.Id, site.Url, site.DailyLimitInMinutes),
			errors => (ErrorOr<SiteResult>)errors);
	}
}
