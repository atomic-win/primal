using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Sites;

namespace Primal.Application.Sites;

internal sealed class AddSiteTimeCommandHandler : IRequestHandler<AddSiteTimeCommand, ErrorOr<Success>>
{
	private readonly ISiteRepository siteRepository;

	private readonly ISiteTimeRepository siteTimeRepository;

	public AddSiteTimeCommandHandler(ISiteRepository siteRepository, ISiteTimeRepository siteTimeRepository)
	{
		this.siteRepository = siteRepository;
		this.siteTimeRepository = siteTimeRepository;
	}

	public async Task<ErrorOr<Success>> Handle(AddSiteTimeCommand request, CancellationToken cancellationToken)
	{
		ErrorOr<Site> site = await this.siteRepository.GetSiteById(request.UserId, request.SiteId, cancellationToken);

		if (site.IsError)
		{
			return site.Errors;
		}

		return await this.siteTimeRepository.AddSiteTime(request.UserId, request.SiteId, request.Time, cancellationToken);
	}
}
