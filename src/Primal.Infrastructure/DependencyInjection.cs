using System.Text;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Primal.Application.Common.Interfaces.Authentication;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Infrastructure.Authentication;
using Primal.Infrastructure.Investments;
using Primal.Infrastructure.Persistence;

namespace Primal.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, ConfigurationManager configuration)
	{
		return services
			.AddSingleton<TimeProvider>(TimeProvider.System)
			.AddRedis(configuration)
			.AddAuthentication(configuration)
			.AddInvestments(configuration)
			.AddPersistence(configuration);
	}

	private static IServiceCollection AddRedis(this IServiceCollection services, ConfigurationManager configuration)
	{
		services.AddStackExchangeRedisCache(options =>
		{
			options.Configuration = configuration["Redis:ConnectionString"];
		});

		return services;
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

	private static IServiceCollection AddInvestments(this IServiceCollection services, ConfigurationManager configuration)
	{
		var investmentSettings = new InvestmentSettings();
		configuration.GetSection(InvestmentSettings.SectionName).Bind(investmentSettings);

		services.AddSingleton(Options.Create(investmentSettings));

		services.AddHttpClient<IMutualFundApiClient, MutualFundApiClient>(client =>
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
			client.BaseAddress = new Uri($"https://www.alphavantage.co/");
		})
		.ConfigurePrimaryHttpMessageHandler(() =>
		{
			return new SocketsHttpHandler()
			{
				PooledConnectionLifetime = TimeSpan.FromMinutes(15),
			};
		})
		.SetHandlerLifetime(Timeout.InfiniteTimeSpan);

		services.AddSingleton<IStockApiClient>(serviceProvider =>
		{
			return serviceProvider.GetRequiredService<StockApiClient>();
		});

		services.AddSingleton<IExchangeRateProvider>(serviceProvider =>
		{
			return new CachedExchangeRateProvider(
				serviceProvider.GetRequiredService<IDistributedCache>(),
				serviceProvider.GetRequiredService<StockApiClient>());
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

		services.AddSingleton<BlobServiceClient>(serviceProvider =>
		{
			var azureStorageSettings = serviceProvider.GetRequiredService<IOptions<AzureStorageSettings>>().Value;
			return new BlobServiceClient(azureStorageSettings.ConnectionString);
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

		foreach (string blobContainerName in Constants.BlobContainerNames.All)
		{
			services.AddKeyedSingleton<BlobContainerClient>(blobContainerName, (serviceProvider, _) =>
			{
				var blobServiceClient = serviceProvider.GetRequiredService<BlobServiceClient>();
				var blobContainerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);
				blobContainerClient.CreateIfNotExists();
				return blobContainerClient;
			});
		}

		return services
			.AddRepositories();
	}

	private static IServiceCollection AddRepositories(this IServiceCollection services)
	{
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

		services.AddSingleton<ISiteTimeRepository>(serviceProvider =>
		{
			var tableClient = serviceProvider.GetKeyedService<TableClient>(Constants.TableNames.SiteTimes);
			return new SiteTimeRepository(tableClient);
		});

		services.AddSingleton<IInstrumentRepository>(serviceProvider =>
		{
			var instrumentIdMappingTableClient = serviceProvider.GetKeyedService<TableClient>(Constants.TableNames.InstrumentIdMapping);
			var instrumentTableClient = serviceProvider.GetKeyedService<TableClient>(Constants.TableNames.Instruments);
			var instrumentBlobContainerClient = serviceProvider.GetKeyedService<BlobContainerClient>(Constants.BlobContainerNames.Instruments);

			return new InstrumentRepository(instrumentIdMappingTableClient, instrumentTableClient, instrumentBlobContainerClient);
		});

		services.AddSingleton<IAssetRepository>(serviceProvider =>
		{
			var tableClient = serviceProvider.GetKeyedService<TableClient>(Constants.TableNames.Assets);
			return new AssetRepository(tableClient);
		});

		services.AddSingleton<ITransactionRepository>(serviceProvider =>
		{
			var tableClient = serviceProvider.GetKeyedService<TableClient>(Constants.TableNames.Transactions);
			return new TransactionRepository(tableClient);
		});

		return services;
	}
}
