using Mapster;

namespace Primal.Application;

public static class DependencyInjection
{
	public static IServiceCollection AddPresentation(this IServiceCollection services)
	{
		services.AddControllers();
		services.AddMapster();
		return services;
	}
}
