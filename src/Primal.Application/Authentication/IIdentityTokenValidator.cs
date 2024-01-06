using ErrorOr;
using Primal.Domain.Users;

namespace Primal.Application.Authentication;

public interface IIdentityTokenValidator
{
	Task<ErrorOr<IdentityUser>> Validate(string idToken);
}
