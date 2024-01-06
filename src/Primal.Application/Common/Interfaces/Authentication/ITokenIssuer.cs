using ErrorOr;
using Primal.Domain.Users;

namespace Primal.Application.Common.Interfaces.Authentication;

public interface ITokenIssuer
{
	Task<ErrorOr<string>> IssueToken(User user, CancellationToken cancellationToken);
}
