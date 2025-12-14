using Autofac;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Primal.Application.Investments;
using Primal.Application.Users;
using Primal.Domain.Users;
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
			c.Resolve<AppDbContext>()))
			.As<IUserIdRepository>();

		builder.Register(c => new UserRepository(
			c.Resolve<AppDbContext>()))
			.As<UserRepository>();

		builder.Register(c => new CachedUserRepository(
			c.Resolve<HybridCache>(),
			c.Resolve<UserRepository>()))
			.As<IUserRepository>();

		builder.Register(c => new AssetRepository(
			c.Resolve<AppDbContext>()))
			.As<AssetRepository>();

		builder.Register(c => new CachedAssetRepository(
			c.Resolve<HybridCache>(),
			c.Resolve<AssetRepository>()))
			.As<IAssetRepository>();

		builder.Register(c => new AssetItemRepository(
			c.Resolve<AppDbContext>()))
			.As<AssetItemRepository>();

		builder.Register(c => new CachedAssetItemRepository(
			c.Resolve<HybridCache>(),
			c.Resolve<AssetItemRepository>()))
			.As<IAssetItemRepository>();

		builder.Register(c => new TransactionRepository(
			c.Resolve<AppDbContext>()))
			.As<TransactionRepository>();

		builder.Register(c => new CachedTransactionRepository(
			c.Resolve<HybridCache>(),
			c.Resolve<TransactionRepository>()))
			.As<ITransactionRepository>();
	}
}
