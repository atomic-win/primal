using Mapster;
using Primal.Api.Middlewares;

namespace Primal.Application;

public static class DependencyInjection
{
	public static IServiceCollection AddPresentation(this IServiceCollection services)
	{
		services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

		services.AddMiddlewares();
		services.AddControllers();
		services.AddMapster();

		return services;
	}

	private static IServiceCollection AddMiddlewares(this IServiceCollection services)
	{
		services.AddSingleton<UserMiddleware>();
		return services;
	}
}
