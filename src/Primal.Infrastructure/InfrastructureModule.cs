using Autofac;
using Microsoft.Extensions.Configuration;
using Primal.Application.Investments;
using Primal.Application.Users;
using Primal.Infrastructure.Investments;
using Primal.Infrastructure.Persistence;
using Primal.Infrastructure.Users;

namespace Primal.Infrastructure;

public sealed class InfrastructureModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		this.RegisterInvestments(builder);
		this.RegisterPersistence(builder);
	}

	private void RegisterInvestments(ContainerBuilder builder)
	{
		builder.RegisterType<MutualFundApiClient>()
			.SingleInstance();

		builder.Register(c => new StockApiClient(
			apiKey: c.Resolve<IConfiguration>().GetValue<string>("InvestmentSettings:FMPApiKey"),
			httpClientFactory: c.Resolve<IHttpClientFactory>()))
			.SingleInstance();

		builder.Register(c => new CachedAssetApiClient<MutualFund>(
			c.Resolve<MutualFundApiClient>()))
			.As<IAssetApiClient<MutualFund>>()
			.SingleInstance();

		builder.Register(c => new CachedAssetApiClient<Stock>(
			c.Resolve<StockApiClient>()))
			.As<IAssetApiClient<Stock>>()
			.SingleInstance();

		builder.Register(c => new CachedExchangeRateProvider(c.Resolve<StockApiClient>()))
			.As<IExchangeRateProvider>()
			.SingleInstance();
	}

	private void RegisterPersistence(ContainerBuilder builder)
	{
		builder.Register(c => new UserIdRepository(
			c.Resolve<AppDbContext>()))
			.As<IUserIdRepository>()
			.InstancePerLifetimeScope();

		builder.Register(c => new UserRepository(
			c.Resolve<AppDbContext>()))
			.As<IUserRepository>()
			.InstancePerLifetimeScope();

		builder.Register(c => new AssetRepository(
			c.Resolve<AppDbContext>()))
			.As<IAssetRepository>()
			.InstancePerLifetimeScope();

		builder.Register(c => new AssetItemRepository(
			c.Resolve<AppDbContext>()))
			.As<IAssetItemRepository>()
			.InstancePerLifetimeScope();

		builder.Register(c => new TransactionRepository(
			c.Resolve<AppDbContext>()))
			.As<ITransactionRepository>()
			.InstancePerLifetimeScope();
	}
}
