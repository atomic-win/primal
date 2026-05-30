using System.Security.Claims;
using Primal.Domain.Users;

namespace Primal.Api;

internal static class UserContextExtensions
{
	internal static UserId GetUserId<TRequest, TResponse>(this FastEndpoints.Endpoint<TRequest, TResponse> ep)
	{
		return ep.User.GetUserId();
	}

	internal static UserId GetUserId(this IHttpContextAccessor httpContextAccessor)
	{
		return httpContextAccessor.HttpContext.User.GetUserId();
	}

	private static UserId GetUserId(this ClaimsPrincipal user)
	{
		var userIdClaim = user.Claims
			.First(x => string.Equals(x.Type, ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase));

		return new UserId(Guid.Parse(userIdClaim.Value));
	}
}
