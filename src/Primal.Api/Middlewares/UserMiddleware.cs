using System.Security.Claims;
using Primal.Api.Common;
using Primal.Domain.Users;

namespace Primal.Api.Middlewares;

internal sealed class UserMiddleware : IMiddleware
{
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		if (!context.User.Identity.IsAuthenticated)
		{
			await next(context);
			return;
		}

		string userIdString = context.User.Claims.First(x => string.Equals(x.Type, ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase)).Value;

		UserId userId = new UserId(Guid.Parse(userIdString));
		context.SetUserId(userId);

		await next(context);
	}
}
