using Microsoft.Extensions.DependencyInjection;
using Primal.Application.Common.Interfaces.Authentication;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Infrastructure.Authentication;
using Primal.Infrastructure.Persistence;

namespace Primal.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services)
	{
		return services
			.AddAuthentication()
			.AddPersistence();
	}

	private static IServiceCollection AddAuthentication(this IServiceCollection services)
	{
		services.AddSingleton<IIdentityTokenValidator, IdentityTokenValidator>();
		services.AddSingleton<ITokenIssuer, TokenIssuer>();

		return services;
	}

	private static IServiceCollection AddPersistence(this IServiceCollection services)
	{
		services.AddSingleton<IUserIdRepository, UserIdRepository>();
		services.AddSingleton<IUserRepository, UserRepository>();

		return services;
	}
}
