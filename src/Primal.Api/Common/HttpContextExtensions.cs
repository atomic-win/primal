using Primal.Domain.Users;

namespace Primal.Api.Common;

internal static class HttpContextExtensions
{
	internal static void SetUserId(this HttpContext httpContext, UserId userId)
	{
		httpContext.Items[nameof(UserId)] = userId;
	}

	internal static UserId GetUserId(this HttpContext httpContext)
	{
		return (UserId)httpContext.Items[nameof(UserId)];
	}
}
