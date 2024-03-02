using ErrorOr;
using Primal.Domain.Sites;
using Primal.Domain.Users;

namespace Primal.Application.Common.Interfaces.Persistence;

public interface ISiteRepository
{
	Task<ErrorOr<Site>> AddSite(UserId userId, string host, int dailyLimitInMinutes, CancellationToken cancellationToken);

	Task<ErrorOr<IEnumerable<Site>>> GetSites(UserId userId, CancellationToken cancellationToken);

	Task<ErrorOr<Site>> GetSite(UserId userId, SiteId siteId, CancellationToken cancellationToken);

	Task<ErrorOr<Success>> UpdateSite(UserId userId, SiteId siteId, int dailyLimitInMinutes, CancellationToken cancellationToken);

	Task<ErrorOr<Success>> DeleteSite(UserId userId, SiteId siteId, CancellationToken cancellationToken);
}
