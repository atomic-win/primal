using ErrorOr;
using Google.Apis.Auth;
using Primal.Application.Common.Interfaces.Authentication;
using Primal.Domain.Common.Errors;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Authentication;

internal sealed class IdTokenValidator : IIdTokenValidator
{
	public async Task<ErrorOr<IdentityProviderUser>> Validate(string idToken)
	{
		try
		{
			GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
			return new IdentityProviderUser(
				new IdentityProviderUserId(payload.Subject),
				IdentityProvider.Google,
				payload.Email,
				payload.GivenName,
				payload.FamilyName,
				payload.Name,
				new Uri(payload.Picture));
		}
		catch (InvalidJwtException ex) when (string.Equals(ex.Message, "JWT has expired.", StringComparison.OrdinalIgnoreCase))
		{
			return Errors.IdToken.Expired;
		}
		catch (InvalidJwtException)
		{
			return Errors.IdToken.Invalid;
		}
		catch (Exception ex)
		{
			return Error.Unexpected(ex.Message);
		}
	}
}
