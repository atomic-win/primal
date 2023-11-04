using ErrorOr;
using Google.Apis.Auth;
using Primal.Application.Authentication;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Authentication;

internal sealed class IdentityProvider : IIdentityProvider
{
	public async Task<ErrorOr<IdentityUser>> Get(string idToken)
	{
		GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

		return new IdentityUser(new IdentityUserId(payload.Subject), payload.Email);
	}
}
