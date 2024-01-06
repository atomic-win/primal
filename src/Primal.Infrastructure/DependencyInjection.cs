using Microsoft.Extensions.DependencyInjection;
using Primal.Application.Authentication;
using Primal.Infrastructure.Authentication;

namespace Primal.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services)
	{
		services.AddSingleton<IIdentityTokenValidator, IdentityTokenValidator>();
		return services;
	}
}
