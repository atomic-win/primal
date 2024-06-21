using Autofac;
using Azure.Data.Tables;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Primal.Application.Common.Interfaces.Authentication;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Infrastructure.Authentication;
using Primal.Infrastructure.Investments;
using Primal.Infrastructure.Persistence;

namespace Primal.Infrastructure;

public sealed class InfrastructureModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterInstance(TimeProvider.System)
			.As<TimeProvider>()
			.SingleInstance();

		this.RegisterAuthentication(builder);
		this.RegisterInvestments(builder);
		this.RegisterPersistence(builder);
	}

	private void RegisterAuthentication(ContainerBuilder builder)
	{
		builder.RegisterType<IdTokenValidator>()
			.As<IIdTokenValidator>()
			.SingleInstance();

		builder.Register(c => new TokenIssuer(
			tokenIssuerSettings: c.Resolve<IOptions<TokenIssuerSettings>>(),
			timeProvider: c.Resolve<TimeProvider>()))
			.As<ITokenIssuer>()
			.SingleInstance();
	}

	private void RegisterInvestments(ContainerBuilder builder)
	{
		builder.Register(c => new CachedMutualFundApiClient(
			c.Resolve<IDistributedCache>(),
			c.Resolve<MutualFundApiClient>()))
			.As<IMutualFundApiClient>();

		builder.Register(c => new CachedStockApiClient(
			c.Resolve<IDistributedCache>(),
			c.Resolve<StockApiClient>()))
			.As<IStockApiClient>();

		builder.Register(c => new CachedExchangeRateProvider(
			c.Resolve<IDistributedCache>(),
			c.Resolve<StockApiClient>()))
			.As<IExchangeRateProvider>();
	}

	private void RegisterPersistence(ContainerBuilder builder)
	{
		builder.Register(c => new TableServiceClient(
			connectionString: c.Resolve<IOptions<AzureStorageSettings>>().Value.ConnectionString))
			.As<TableServiceClient>()
			.SingleInstance();

		foreach (string tableName in Constants.TableNames.All)
		{
			builder.Register(c =>
			{
				var tableServiceClient = c.Resolve<TableServiceClient>();
				tableServiceClient.CreateTableIfNotExists(tableName);
				return tableServiceClient.GetTableClient(tableName);
			})
			.Keyed<TableClient>(tableName)
			.SingleInstance();
		}

		builder.Register(c => new UserIdRepository(
			c.ResolveKeyed<TableClient>(Constants.TableNames.UserIds)))
			.As<IUserIdRepository>()
			.SingleInstance();

		builder.Register(c => new UserRepository(
			c.ResolveKeyed<TableClient>(Constants.TableNames.Users)))
			.As<IUserRepository>()
			.SingleInstance();

		builder.Register(c => new SiteRepository(
			c.ResolveKeyed<TableClient>(Constants.TableNames.Sites)))
			.As<ISiteRepository>()
			.SingleInstance();

		builder.Register(c => new SiteTimeRepository(
			c.ResolveKeyed<TableClient>(Constants.TableNames.SiteTimes)))
			.As<ISiteTimeRepository>()
			.SingleInstance();

		builder.Register(c => new InstrumentRepository(
			c.ResolveKeyed<TableClient>(Constants.TableNames.InstrumentIdMapping),
			c.ResolveKeyed<TableClient>(Constants.TableNames.Instruments)))
			.As<IInstrumentRepository>()
			.SingleInstance();

		builder.Register(c => new AssetRepository(
			c.ResolveKeyed<TableClient>(Constants.TableNames.Assets)))
			.As<IAssetRepository>()
			.SingleInstance();

		builder.Register(c => new TransactionRepository(
			c.ResolveKeyed<TableClient>(Constants.TableNames.Transactions)))
			.As<ITransactionRepository>()
			.SingleInstance();
	}
}
