using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Primal.Infrastructure.Authentication;
using Primal.Infrastructure.Investments;
using Primal.Infrastructure.Persistence;

namespace Primal.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, ConfigurationManager configuration)
	{
		return services
			.AddAuthentication(configuration)
			.AddInvestments(configuration)
			.AddPersistence(configuration);
	}

	private static IServiceCollection AddAuthentication(this IServiceCollection services, ConfigurationManager configuration)
	{
		var tokenIssuerSettings = new TokenIssuerSettings();
		configuration.GetSection(TokenIssuerSettings.SectionName).Bind(tokenIssuerSettings);

		services.AddSingleton(Options.Create(tokenIssuerSettings));

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

		return services;
	}

	private static IServiceCollection AddPersistence(this IServiceCollection services, ConfigurationManager configuration)
	{
		services.Configure<PersistenceSettings>(configuration.GetSection(PersistenceSettings.SectionName));
		return services;
	}
}
