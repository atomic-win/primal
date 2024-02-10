using ErrorOr;
using Primal.Domain.Users;

namespace Primal.Application.Common.Interfaces.Persistence;

public interface IUserIdRepository
{
	Task<ErrorOr<UserId>> GetUserId(IdentityProviderUser identityProviderUser, CancellationToken cancellationToken);

	Task<ErrorOr<Success>> AddUserId(IdentityProviderUser identityProviderUser, UserId userId, CancellationToken cancellationToken);
}
