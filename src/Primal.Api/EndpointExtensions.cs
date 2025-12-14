using System.Security.Claims;
using Primal.Domain.Users;

namespace Primal.Api;

internal static class EndpointExtensions
{
	internal static UserId GetUserId<TRequest, TResponse>(this FastEndpoints.Endpoint<TRequest, TResponse> ep)
	{
		string userIdString = ep.User.Claims.First(x => string.Equals(x.Type, ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase)).Value;

		UserId userId = new UserId(Guid.Parse(userIdString));
		return userId;
	}
}
