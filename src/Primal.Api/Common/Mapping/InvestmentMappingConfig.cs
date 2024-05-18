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
		config.NewConfig<(UserId UserId, AddMutualFundAssetRequest AddMutualFundAssetRequest), AddMutualFundAssetCommand>()
			.Map(dest => dest.UserId, src => src.UserId)
			.Map(dest => dest, src => src.AddMutualFundAssetRequest);

		config.NewConfig<(UserId UserId, AddStockAssetRequest AddStockAssetRequest), AddStockAssetCommand>()
			.Map(dest => dest.UserId, src => src.UserId)
			.Map(dest => dest, src => src.AddStockAssetRequest);

		config.NewConfig<Asset, AssetResponse>()
			.Map(dest => dest.Id, src => src.Id.Value)
			.Map(dest => dest.InstrumentId, src => src.InstrumentId.Value);

		config.NewConfig<MutualFund, MutualFundResponse>()
			.Map(dest => dest.Id, src => src.Id.Value);

		config.NewConfig<Stock, StockResponse>()
			.Map(dest => dest.Id, src => src.Id.Value);

		config.NewConfig<InvestmentInstrument, InstrumentResponse>()
			.Map(dest => dest.Id, src => src.Id.Value)
			.Include<MutualFund, MutualFundResponse>()
			.Include<Stock, StockResponse>();
	}
}
