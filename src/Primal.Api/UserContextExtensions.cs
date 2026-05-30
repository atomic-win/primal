using System.Security.Claims;
using Primal.Domain.Users;

namespace Primal.Api;

internal static class UserContextExtensions
{
	internal static UserId GetUserId<TRequest, TResponse>(this FastEndpoints.Endpoint<TRequest, TResponse> ep)
	{
		string userIdString = ep.User.Claims.First(x => string.Equals(x.Type, ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase)).Value;

		UserId userId = new UserId(Guid.Parse(userIdString));
		return userId;
	}

	internal static UserId GetUserId(this IHttpContextAccessor httpContextAccessor)
	{
		var userIdClaim = httpContextAccessor.HttpContext!.User.Claims
			.First(x => string.Equals(x.Type, ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase));
		return new UserId(Guid.Parse(userIdClaim.Value));
	}
}
