using Autofac;
using LiteDB;
using Microsoft.Extensions.Options;
using Primal.Application.Common.Interfaces;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Application.Users;
using Primal.Infrastructure.Common;
using Primal.Infrastructure.Investments;
using Primal.Infrastructure.Persistence;
using Primal.Infrastructure.Users;

namespace Primal.Infrastructure;

public sealed class InfrastructureModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterInstance(TimeProvider.System)
			.As<TimeProvider>()
			.SingleInstance();

		this.RegisterInvestments(builder);
		this.RegisterPersistence(builder);
	}

	private void RegisterInvestments(ContainerBuilder builder)
	{
		builder.Register(c => new CachedMutualFundApiClient(
			c.Resolve<ICache>(),
			c.Resolve<MutualFundApiClient>()))
			.As<IMutualFundApiClient>();

		builder.Register(c => new CachedStockApiClient(
			c.Resolve<ICache>(),
			c.Resolve<StockApiClient>()))
			.As<IStockApiClient>();

		builder.Register(c => new CachedExchangeRateProvider(
			c.Resolve<ICache>(),
			c.Resolve<StockApiClient>()))
			.As<IExchangeRateProvider>();
	}

	private void RegisterPersistence(ContainerBuilder builder)
	{
		builder.Register(c => new LiteDatabase(
			c.Resolve<IOptions<PersistenceSettings>>().Value.LiteDB.FilePath))
			.As<LiteDatabase>()
			.SingleInstance();

		builder.Register(c => new LiteDbCache(
			c.Resolve<LiteDatabase>()))
			.As<ICache>()
			.SingleInstance();

		builder.Register(c => new UserIdRepository(
			c.Resolve<AppDbContext>()))
			.As<IUserIdRepository>()
			.InstancePerLifetimeScope();

		builder.Register(c => new UserRepository(
			c.Resolve<AppDbContext>()))
			.As<IUserRepository>()
			.InstancePerLifetimeScope();

		builder.Register(c => new AssetRepository(
			c.Resolve<LiteDatabase>()))
			.As<IAssetRepository>()
			.SingleInstance();

		builder.Register(c => new InstrumentRepository(
			c.Resolve<LiteDatabase>()))
			.As<IInstrumentRepository>()
			.SingleInstance();

		builder.Register(c => new TransactionRepository(
			c.Resolve<LiteDatabase>()))
			.As<ITransactionRepository>()
			.SingleInstance();
	}
}
