using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Primal.Infrastructure.Investments;

namespace Primal.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, ConfigurationManager configuration)
	{
		return services
			.AddInvestments(configuration);
	}

	private static IServiceCollection AddInvestments(this IServiceCollection services, ConfigurationManager configuration)
	{
		services.AddHttpClient<MutualFundApiClient>(client =>
		{
			client.BaseAddress = new Uri("https://api.mfapi.in");
		})
		.ConfigurePrimaryHttpMessageHandler(() =>
		{
			return new SocketsHttpHandler()
			{
				PooledConnectionLifetime = TimeSpan.FromMinutes(15),
			};
		})
		.SetHandlerLifetime(Timeout.InfiniteTimeSpan);

		services.AddHttpClient<StockApiClient>(client =>
		{
			client.BaseAddress = new Uri("https://financialmodelingprep.com");
		})
		.ConfigurePrimaryHttpMessageHandler(() =>
		{
			return new SocketsHttpHandler()
			{
				PooledConnectionLifetime = TimeSpan.FromMinutes(15),
			};
		})
		.SetHandlerLifetime(Timeout.InfiniteTimeSpan);

		return services;
	}
}
