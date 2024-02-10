using ErrorOr;
using Primal.Domain.Users;

namespace Primal.Application.Common.Interfaces.Persistence;

public interface IUserRepository
{
	Task<ErrorOr<User>> GetUser(UserId userId, CancellationToken cancellationToken);

	Task<ErrorOr<Success>> AddUser(User user, CancellationToken cancellationToken);
}
