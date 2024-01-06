using ErrorOr;
using Google.Apis.Auth;
using Primal.Application.Common.Interfaces.Authentication;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Authentication;

internal sealed class IdentityTokenValidator : IIdentityTokenValidator
{
	public async Task<ErrorOr<IdentityProviderUser>> Validate(string idToken)
	{
		GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

		return new IdentityProviderUser(new IdentityProviderUserId(payload.Subject), IdentityProvider.Google, payload.Email);
	}
}
