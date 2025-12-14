using Primal.Domain.Users;

namespace Primal.Application.Users;

public interface IUserIdRepository
{
	Task<UserId> GetUserId(
		IdentityProvider identityProvider,
		IdentityProviderUserId identityProviderUserId,
		CancellationToken cancellationToken);

	Task<UserId> AddUserId(
		IdentityProvider identityProvider,
		IdentityProviderUserId identityProviderUserId,
		CancellationToken cancellationToken);
}
