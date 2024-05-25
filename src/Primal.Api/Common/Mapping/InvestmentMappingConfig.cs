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
	}

	private static void RegisterAssetMappings(TypeAdapterConfig config)
	{
		config.NewConfig<Guid, AssetId>()
			.ConstructUsing(src => new AssetId(src));

		config.NewConfig<(UserId UserId, AddCashDepositAssetRequest AddCashDepositAssetRequest), AddCashDepositAssetCommand>()
					.Map(dest => dest.UserId, src => src.UserId)
					.Map(dest => dest, src => src.AddCashDepositAssetRequest);

		config.NewConfig<(UserId UserId, AddMutualFundAssetRequest AddMutualFundAssetRequest), AddMutualFundAssetCommand>()
			.Map(dest => dest.UserId, src => src.UserId)
			.Map(dest => dest, src => src.AddMutualFundAssetRequest);

		config.NewConfig<(UserId UserId, AddStockAssetRequest AddStockAssetRequest), AddStockAssetCommand>()
			.Map(dest => dest.UserId, src => src.UserId)
			.Map(dest => dest, src => src.AddStockAssetRequest);

		config.NewConfig<Asset, AssetResponse>()
			.Map(dest => dest.Id, src => src.Id.Value)
			.Map(dest => dest.InstrumentId, src => src.InstrumentId.Value);
	}

	private static void RegisterInstrumentMappings(TypeAdapterConfig config)
	{
		config.NewConfig<CashDeposit, CashDepositResponse>()
					.Map(dest => dest.Id, src => src.Id.Value);

		config.NewConfig<MutualFund, MutualFundResponse>()
			.Map(dest => dest.Id, src => src.Id.Value);

		config.NewConfig<Stock, StockResponse>()
			.Map(dest => dest.Id, src => src.Id.Value);

		config.NewConfig<InvestmentInstrument, InstrumentResponse>()
			.Map(dest => dest.Id, src => src.Id.Value)
			.Include<CashDeposit, CashDepositResponse>()
			.Include<MutualFund, MutualFundResponse>()
			.Include<Stock, StockResponse>();
	}

	private static void RegisterTransactionMappings(TypeAdapterConfig config)
	{
		config.NewConfig<(UserId UserId, BuySellRequest BuySellRequest), AddBuySellTransactionCommand>()
			.Map(dest => dest.UserId, src => src.UserId)
			.Map(dest => dest, src => src.BuySellRequest);

		config.NewConfig<BuySellTransaction, BuySellResponse>()
			.Map(dest => dest.Id, src => src.Id.Value)
			.Map(dest => dest.AssetId, src => src.AssetId.Value);

		config.NewConfig<Transaction, TransactionResponse>()
			.Map(dest => dest.Id, src => src.Id.Value)
			.Map(dest => dest.AssetId, src => src.AssetId.Value)
			.Include<BuySellTransaction, BuySellResponse>();
	}
}
