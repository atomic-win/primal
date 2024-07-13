using ErrorOr;
using Primal.Domain.Users;

namespace Primal.Application.Common.Interfaces.Persistence;

public interface IUserIdRepository
{
	Task<ErrorOr<UserId>> GetUserId(
		IdentityProvider identityProvider,
		IdentityProviderUserId identityProviderUserId,
		CancellationToken cancellationToken);

	Task<ErrorOr<UserId>> AddUserId(
		IdentityProvider identityProvider,
		IdentityProviderUserId identityProviderUserId,
		CancellationToken cancellationToken);
}
