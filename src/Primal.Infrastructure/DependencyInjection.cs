using System.Text;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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
			.AddAuthentication(configuration)
			.AddPersistence(configuration);
	}

	private static IServiceCollection AddAuthentication(this IServiceCollection services, ConfigurationManager configuration)
	{
		var tokenIssuerSettings = new TokenIssuerSettings();
		configuration.GetSection(TokenIssuerSettings.SectionName).Bind(tokenIssuerSettings);

		services.AddSingleton(Options.Create(tokenIssuerSettings));

		services.AddSingleton<IIdTokenValidator, IdTokenValidator>();

		services.AddSingleton<ITokenIssuer>(serviceProvider =>
		{
			return new TokenIssuer(
				tokenIssuerSettings: serviceProvider.GetRequiredService<IOptions<TokenIssuerSettings>>(),
				timeProvider: serviceProvider.GetRequiredService<TimeProvider>());
		});

		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.RequireHttpsMetadata = false;
				options.SaveToken = true;

				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = tokenIssuerSettings.Issuer,
					ValidAudience = tokenIssuerSettings.Audience,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenIssuerSettings.SecretKey)),
				};
			});

		return services;
	}

	private static IServiceCollection AddPersistence(this IServiceCollection services, ConfigurationManager configuration)
	{
		services.Configure<AzureStorageSettings>(configuration.GetSection(AzureStorageSettings.SectionName));

		services.AddSingleton<TableServiceClient>(serviceProvider =>
		{
			var azureStorageSettings = serviceProvider.GetRequiredService<IOptions<AzureStorageSettings>>().Value;
			return new TableServiceClient(azureStorageSettings.ConnectionString);
		});

		foreach (string tableName in Constants.TableNames.All)
		{
			services.AddKeyedSingleton<TableClient>(tableName, (serviceProvider, _) =>
			{
				var tableServiceClient = serviceProvider.GetRequiredService<TableServiceClient>();
				tableServiceClient.CreateTableIfNotExists(tableName);
				return tableServiceClient.GetTableClient(tableName);
			});
		}

		services.AddSingleton<IUserIdRepository>(serviceProvider =>
		{
			var tableClient = serviceProvider.GetKeyedService<TableClient>(Constants.TableNames.UserIds);
			return new UserIdRepository(tableClient);
		});

		services.AddSingleton<IUserRepository>(serviceProvider =>
		{
			var tableClient = serviceProvider.GetKeyedService<TableClient>(Constants.TableNames.Users);
			return new UserRepository(tableClient);
		});

		services.AddSingleton<ISiteRepository>(serviceProvider =>
		{
			var tableClient = serviceProvider.GetKeyedService<TableClient>(Constants.TableNames.Sites);
			return new SiteRepository(tableClient);
		});

		return services;
	}
}
