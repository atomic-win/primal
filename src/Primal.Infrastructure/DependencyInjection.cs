using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Primal.Infrastructure.Investments;
using Primal.Infrastructure.Persistence;

namespace Primal.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, ConfigurationManager configuration)
	{
		return services
			.AddPersistence(configuration)
			.AddInvestments(configuration);
	}

	private static IServiceCollection AddPersistence(this IServiceCollection services, ConfigurationManager configuration)
	{
		var connectionFactory = new DbConnectionFactory(
			configuration.GetConnectionString("DefaultConnection")!);

		DatabaseInitializer.Initialize(connectionFactory);

		services.AddSingleton(connectionFactory);

		return services;
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

		services.AddHttpClient<ExchangeRateApiClient>(client =>
		{
			client.BaseAddress = new Uri("https://www.alphavantage.co");
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
