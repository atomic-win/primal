using ErrorOr;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Persistence;

internal sealed class UserIdRepository : IUserIdRepository
{
	public async Task<ErrorOr<UserId>> GetUserId(IdentityProviderUser identityProviderUser, CancellationToken cancellationToken)
	{
		await Task.CompletedTask;
		return new UserId(Guid.NewGuid());
	}
}
