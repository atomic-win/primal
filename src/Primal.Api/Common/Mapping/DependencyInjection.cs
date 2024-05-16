using System.Reflection;
using Mapster;
using MapsterMapper;

namespace Primal.Api.Common.Mapping;

internal static class DependencyInjection
{
	internal static IServiceCollection AddMappings(this IServiceCollection services)
	{
		var config = TypeAdapterConfig.GlobalSettings;
		config.Scan(Assembly.GetExecutingAssembly());

		return services
			.AddSingleton(config)
			.AddSingleton<IMapper, ServiceMapper>();
	}
}
