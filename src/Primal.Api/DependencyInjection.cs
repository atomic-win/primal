using Hangfire;
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

		// Add Hangfire services
		services.AddHangfire(configuration => configuration
			.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
			.UseSimpleAssemblyNameTypeSerializer()
			.UseRecommendedSerializerSettings()
			.UseInMemoryStorage());

		// Add the processing server as IHostedService
		services.AddHangfireServer();

		return services;
	}

	private static IServiceCollection AddMiddlewares(this IServiceCollection services)
	{
		services.AddSingleton<UserMiddleware>();
		return services;
	}
}
