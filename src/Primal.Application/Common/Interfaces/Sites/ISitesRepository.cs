using ErrorOr;
using Primal.Domain.Sites;
using Primal.Domain.Users;

namespace Primal.Application.Common.Interfaces.Sites;

public interface ISitesRepository
{
	Task<ErrorOr<Site>> AddSite(UserId userId, Uri url, int dailyLimitInMinutes);
}
