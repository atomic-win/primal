using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Primal.Application.Common.Interfaces.Authentication;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Infrastructure.Authentication;
using Primal.Infrastructure.Persistence;

namespace Primal.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, ConfigurationManager configuration)
	{
		return services
			.AddSingleton<TimeProvider>(TimeProvider.System)
			.AddConfiguration(configuration)
			.AddAuthentication()
			.AddPersistence();
	}

	private static IServiceCollection AddConfiguration(this IServiceCollection services, ConfigurationManager configuration)
	{
		services.Configure<TokenIssuerSettings>(configuration.GetSection(TokenIssuerSettings.SectionName));
		services.Configure<AzureStorageSettings>(configuration.GetSection(AzureStorageSettings.SectionName));

		return services;
	}

	private static IServiceCollection AddAuthentication(this IServiceCollection services)
	{
		services.AddSingleton<IIdentityTokenValidator, IdentityTokenValidator>();
		services.AddSingleton<ITokenIssuer>(serviceProvider =>
		{
			return new TokenIssuer(
				tokenIssuerSettings: serviceProvider.GetRequiredService<IOptions<TokenIssuerSettings>>(),
				timeProvider: serviceProvider.GetRequiredService<TimeProvider>());
		});

		return services;
	}

	private static IServiceCollection AddPersistence(this IServiceCollection services)
	{
		services.AddSingleton<TableServiceClient>(serviceProvider =>
		{
			var azureStorageSettings = serviceProvider.GetRequiredService<IOptions<AzureStorageSettings>>().Value;
			return new TableServiceClient(azureStorageSettings.ConnectionString);
		});

		services.AddSingleton<TableClient>(serviceProvider =>
		{
			var tableServiceClient = serviceProvider.GetRequiredService<TableServiceClient>();
			tableServiceClient.CreateTableIfNotExists("Default");
			return tableServiceClient.GetTableClient("Default");
		});

		services.AddSingleton<IUserIdRepository>(serviceProvider =>
		{
			var tableClient = serviceProvider.GetRequiredService<TableClient>();
			return new UserIdRepository(tableClient);
		});

		services.AddSingleton<IUserRepository>(serviceProvider =>
		{
			var tableClient = serviceProvider.GetRequiredService<TableClient>();
			return new UserRepository(tableClient);
		});

		return services;
	}
}
