namespace Primal.Api.Middlewares;

internal static class MiddlewareExtensions
{
	internal static IApplicationBuilder UseUserMiddleware(this IApplicationBuilder app)
	{
		app.UseMiddleware<UserMiddleware>();
		return app;
	}
}
