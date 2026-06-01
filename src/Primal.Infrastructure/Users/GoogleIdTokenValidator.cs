using Google.Apis.Auth;
using Primal.Application.Users;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Users;

internal sealed class GoogleIdTokenValidator : IIdTokenValidator
{
	public async Task<IdentityProviderUser> ValidateAsync(string idToken, CancellationToken cancellationToken)
	{
		var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

		return new IdentityProviderUser(
			id: new IdentityProviderUserId(payload.Subject),
			identityProvider: IdentityProvider.Google,
			email: payload.Email,
			firstName: payload.GivenName,
			lastName: payload.FamilyName,
			fullName: payload.Name,
			profilePictureUrl: new Uri(payload.Picture ?? string.Empty, UriKind.RelativeOrAbsolute));
	}
}
