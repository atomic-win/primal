using Autofac;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;

using InvestmentPortfolioTracker.Core.Investments;
using InvestmentPortfolioTracker.Core.Users;
using InvestmentPortfolioTracker.Infrastructure.Investments;
using InvestmentPortfolioTracker.Infrastructure.Persistence;
using InvestmentPortfolioTracker.Infrastructure.Users;

namespace InvestmentPortfolioTracker.Infrastructure;

public sealed class InfrastructureModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		this.RegisterInvestments(builder);
		this.RegisterPersistence(builder);
	}

	private void RegisterInvestments(ContainerBuilder builder)
	{
		builder.Register(c => new RateRepository(
			c.Resolve<DbConnectionFactory>(),
			c.Resolve<TimeProvider>()))
			.SingleInstance();

		builder.Register(c => new MutualFundApiClient(
			c.Resolve<IHttpClientFactory>()))
			.SingleInstance();

		builder.Register(c => new AlphaVantageApiClient(
			apiKey: c.Resolve<IConfiguration>().GetValue<string>("InvestmentSettings:AlphaVantageApiKey"),
			httpClientFactory: c.Resolve<IHttpClientFactory>()))
			.SingleInstance();

		builder.Register(c => new CachedAssetApiClient<MutualFund>(
			c.Resolve<HybridCache>(),
			c.Resolve<MutualFundApiClient>(),
			c.Resolve<RateRepository>(),
			RateType.MutualFund))
			.As<IAssetApiClient<MutualFund>>()
			.SingleInstance();

		builder.Register(c => new CachedAssetApiClient<Stock>(
			c.Resolve<HybridCache>(),
			c.Resolve<AlphaVantageApiClient>(),
			c.Resolve<RateRepository>(),
			RateType.Stock))
			.As<IAssetApiClient<Stock>>()
			.SingleInstance();

		builder.Register(c => new CachedForexApiClient(
			c.Resolve<HybridCache>(),
			c.Resolve<AlphaVantageApiClient>(),
			c.Resolve<RateRepository>()))
			.As<IForexApiClient>()
			.SingleInstance();
	}

	private void RegisterPersistence(ContainerBuilder builder)
	{
		builder.Register(c => new UserIdRepository(
			c.Resolve<DbConnectionFactory>(),
			c.Resolve<TimeProvider>()))
			.As<IUserIdRepository>()
			.SingleInstance();

		builder.Register(c => new UserRepository(
			c.Resolve<DbConnectionFactory>(),
			c.Resolve<TimeProvider>()))
			.As<UserRepository>()
			.SingleInstance();

		builder.Register(c => new CachedUserRepository(
			c.Resolve<HybridCache>(),
			c.Resolve<UserRepository>()))
			.As<IUserRepository>()
			.SingleInstance();

		builder.Register(c => new AssetRepository(
			c.Resolve<DbConnectionFactory>(),
			c.Resolve<TimeProvider>()))
			.As<AssetRepository>()
			.SingleInstance();

		builder.Register(c => new CachedAssetRepository(
			c.Resolve<HybridCache>(),
			c.Resolve<AssetRepository>()))
			.As<IAssetRepository>()
			.SingleInstance();

		builder.Register(c => new AssetItemRepository(
			c.Resolve<DbConnectionFactory>(),
			c.Resolve<TimeProvider>()))
			.As<AssetItemRepository>()
			.SingleInstance();

		builder.Register(c => new CachedAssetItemRepository(
			c.Resolve<HybridCache>(),
			c.Resolve<AssetItemRepository>()))
			.As<IAssetItemRepository>()
			.SingleInstance();

		builder.Register(c => new TransactionRepository(
			c.Resolve<DbConnectionFactory>(),
			c.Resolve<TimeProvider>()))
			.As<TransactionRepository>()
			.SingleInstance();

		builder.Register(c => new CachedTransactionRepository(
			c.Resolve<HybridCache>(),
			c.Resolve<TimeProvider>(),
			c.Resolve<TransactionRepository>()))
			.As<ITransactionRepository>()
			.SingleInstance();
	}
}
