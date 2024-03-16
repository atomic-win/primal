using ErrorOr;
using Primal.Domain.Sites;
using Primal.Domain.Users;

namespace Primal.Application.Common.Interfaces.Persistence;

public interface ISiteTimeRepository
{
	Task<ErrorOr<Success>> AddSiteTime(UserId userId, SiteId siteId, DateTime time, CancellationToken cancellationToken);
}
