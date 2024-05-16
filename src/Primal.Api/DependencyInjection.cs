using Mapster;
using Primal.Api.Common.Mapping;
using Primal.Api.Middlewares;

namespace Primal.Application;

internal static class DependencyInjection
{
	internal static IServiceCollection AddPresentation(this IServiceCollection services)
	{
		services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		services.AddMiddlewares();
		services.AddControllers().AddNewtonsoftJson();
		services.AddMapster();
		services.AddMappings();

		return services;
	}

	private static IServiceCollection AddMiddlewares(this IServiceCollection services)
	{
		services.AddSingleton<UserMiddleware>();
		return services;
	}
}
