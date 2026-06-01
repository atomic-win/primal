using Primal.Domain.Users;

namespace Primal.Application.Users;

public interface IIdTokenValidator
{
	Task<IdentityProviderUser> ValidateAsync(string idToken, CancellationToken cancellationToken);
}
