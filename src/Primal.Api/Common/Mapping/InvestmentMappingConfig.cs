using Mapster;
using Primal.Application.Investments;
using Primal.Contracts.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Api.Common.Mapping;

internal sealed class InvestmentMappingConfig : IRegister
{
	public void Register(TypeAdapterConfig config)
	{
		RegisterAssetMappings(config);
		RegisterInstrumentMappings(config);
		RegisterTransactionMappings(config);
		RegisterPortfolioMappings(config);
	}

	private static void RegisterAssetMappings(TypeAdapterConfig config)
	{
		config.NewConfig<Guid, AssetId>()
			.ConstructUsing(src => new AssetId(src));

		config.NewConfig<AssetId, Guid>()
			.ConstructUsing(src => src.Value);

		config.NewConfig<(UserId UserId, AddCashDepositAssetRequest AddCashDepositAssetRequest), AddCashDepositAssetCommand>()
			.Map(dest => dest.UserId, src => src.UserId)
			.Map(dest => dest, src => src.AddCashDepositAssetRequest);

		config.NewConfig<(UserId UserId, AddMutualFundAssetRequest AddMutualFundAssetRequest), AddMutualFundAssetCommand>()
			.Map(dest => dest.UserId, src => src.UserId)
			.Map(dest => dest, src => src.AddMutualFundAssetRequest);

		config.NewConfig<(UserId UserId, AddStockAssetRequest AddStockAssetRequest), AddStockAssetCommand>()
			.Map(dest => dest.UserId, src => src.UserId)
			.Map(dest => dest, src => src.AddStockAssetRequest);

		config.NewConfig<Asset, AssetResponse>();
	}

	private static void RegisterInstrumentMappings(TypeAdapterConfig config)
	{
		config.NewConfig<Guid, InstrumentId>()
			.ConstructUsing(src => new InstrumentId(src));

		config.NewConfig<InstrumentId, Guid>()
			.ConstructUsing(src => src.Value);

		config.NewConfig<CashDeposit, CashDepositResponse>();
		config.NewConfig<MutualFund, MutualFundResponse>();
		config.NewConfig<Stock, StockResponse>();

		config.NewConfig<InvestmentInstrument, InstrumentResponse>()
			.Include<CashDeposit, CashDepositResponse>()
			.Include<MutualFund, MutualFundResponse>()
			.Include<Stock, StockResponse>();
	}

	private static void RegisterTransactionMappings(TypeAdapterConfig config)
	{
		config.NewConfig<Guid, TransactionId>()
			.ConstructUsing(src => new TransactionId(src));

		config.NewConfig<TransactionId, Guid>()
			.ConstructUsing(src => src.Value);

		config.NewConfig<(UserId UserId, TransactionRequest TransactionRequest), AddTransactionCommand>()
			.Map(dest => dest.UserId, src => src.UserId)
			.Map(dest => dest, src => src.TransactionRequest);

		config.NewConfig<Transaction, TransactionResponse>();
	}

	private static void RegisterPortfolioMappings(TypeAdapterConfig config)
	{
		config.NewConfig<Portfolio<AssetId>, PortfolioResponse<Guid>>();
		config.NewConfig<Portfolio<InstrumentId>, PortfolioResponse<Guid>>();
		config.NewConfig<Portfolio<InstrumentType>, PortfolioResponse<string>>();
		config.NewConfig<Portfolio<string>, PortfolioResponse<string>>();
	}
}
