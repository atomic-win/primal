using ErrorOr;
using Primal.Domain.Users;

namespace Primal.Application.Common.Interfaces.Authentication;

public interface IIdTokenValidator
{
	Task<ErrorOr<IdentityProviderUser>> Validate(string idToken);
}
