using ErrorOr;
using Primal.Domain.Users;

namespace Primal.Application.Authentication;

public interface IIdentityProvider
{
	Task<ErrorOr<IdentityUser>> Get(string idToken);
}
