using ErrorOr;
using Primal.Domain.Users;

namespace Primal.Application.Common.Interfaces.Persistence;

public interface IUserRepository
{
	Task<ErrorOr<User>> GetUserAsync(UserId userId, CancellationToken cancellationToken);

	Task<ErrorOr<Success>> AddUserAsync(User user, CancellationToken cancellationToken);
}
