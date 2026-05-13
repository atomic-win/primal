using Autofac;
using Microsoft.Extensions.Caching.Hybrid;
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
		builder.Register(c => new MutualFundApiClient(
			c.Resolve<IHttpClientFactory>()))
			.SingleInstance();

		builder.Register(c => new StockApiClient(
			apiKey: c.Resolve<IConfiguration>().GetValue<string>("InvestmentSettings:FMPApiKey"),
			httpClientFactory: c.Resolve<IHttpClientFactory>()))
			.SingleInstance();

		builder.Register(c => new CachedAssetApiClient<MutualFund>(
			c.Resolve<HybridCache>(),
			c.Resolve<MutualFundApiClient>()))
			.As<IAssetApiClient<MutualFund>>()
			.SingleInstance();

		builder.Register(c => new CachedAssetApiClient<Stock>(
			c.Resolve<HybridCache>(),
			c.Resolve<StockApiClient>()))
			.As<IAssetApiClient<Stock>>()
			.SingleInstance();

		builder.Register(c => new ExchangeRateApiClient(
			c.Resolve<IConfiguration>().GetValue<string>("InvestmentSettings:AlphaVantageApiKey"),
			c.Resolve<IHttpClientFactory>()))
			.SingleInstance();

		builder.Register(c => new CachedExchangeRateApiClient(
			c.Resolve<HybridCache>(),
			c.Resolve<ExchangeRateApiClient>()))
			.As<IExchangeRateApiClient>()
			.SingleInstance();
	}

	private void RegisterPersistence(ContainerBuilder builder)
	{
		builder.Register(c => new UserIdRepository(
			c.Resolve<DbConnectionFactory>()))
			.As<IUserIdRepository>()
			.InstancePerLifetimeScope();

		builder.Register(c => new UserRepository(
			c.Resolve<DbConnectionFactory>()))
			.As<UserRepository>()
			.InstancePerLifetimeScope();

		builder.Register(c => new CachedUserRepository(
			c.Resolve<HybridCache>(),
			c.Resolve<UserRepository>()))
			.As<IUserRepository>()
			.InstancePerLifetimeScope();

		builder.Register(c => new AssetRepository(
			c.Resolve<DbConnectionFactory>()))
			.As<AssetRepository>()
			.InstancePerLifetimeScope();

		builder.Register(c => new CachedAssetRepository(
			c.Resolve<HybridCache>(),
			c.Resolve<AssetRepository>()))
			.As<IAssetRepository>()
			.InstancePerLifetimeScope();

		builder.Register(c => new AssetItemRepository(
			c.Resolve<DbConnectionFactory>()))
			.As<AssetItemRepository>()
			.InstancePerLifetimeScope();

		builder.Register(c => new CachedAssetItemRepository(
			c.Resolve<HybridCache>(),
			c.Resolve<AssetItemRepository>()))
			.As<IAssetItemRepository>()
			.InstancePerLifetimeScope();

		builder.Register(c => new TransactionRepository(
			c.Resolve<DbConnectionFactory>()))
			.As<TransactionRepository>()
			.InstancePerLifetimeScope();

		builder.Register(c => new CachedTransactionRepository(
			c.Resolve<HybridCache>(),
			c.Resolve<TransactionRepository>()))
			.As<ITransactionRepository>()
			.InstancePerLifetimeScope();
	}
}
