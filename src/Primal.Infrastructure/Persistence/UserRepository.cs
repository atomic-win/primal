using ErrorOr;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Persistence;

internal sealed class UserRepository : IUserRepository
{
	public async Task<ErrorOr<User>> GetUser(UserId userId, IdentityProviderUser identityProviderUser, CancellationToken cancellationToken)
	{
		await Task.CompletedTask;
		return new User(userId, identityProviderUser.Email);
	}
}
