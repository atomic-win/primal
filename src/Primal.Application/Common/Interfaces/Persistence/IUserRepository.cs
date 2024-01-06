using ErrorOr;
using Primal.Domain.Users;

namespace Primal.Application.Common.Interfaces.Persistence;

public interface IUserRepository
{
	Task<ErrorOr<User>> GetUser(UserId userId, IdentityProviderUser identityProviderUser, CancellationToken cancellationToken);
}
