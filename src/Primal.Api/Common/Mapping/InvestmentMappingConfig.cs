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
		config.NewConfig<(UserId UserId, AddMutualFundInstrumentRequest AddMutualFundInstrumentRequest), AddMutualFundInstrumentCommand>()
			.Map(dest => dest.UserId, src => src.UserId)
			.Map(dest => dest, src => src.AddMutualFundInstrumentRequest);

		config.NewConfig<MutualFundInstrument, MutualFundInstrumentResponse>()
			.Map(dest => dest.Id, src => src.Id.Value)
			.Map(dest => dest.MutualFundId, src => src.MutualFundId.Value);

		config.NewConfig<Instrument, InstrumentResponse>()
			.Map(dest => dest.Id, src => src.Id.Value)
			.Include<MutualFundInstrument, MutualFundInstrumentResponse>();

		config.NewConfig<MutualFund, MutualFundResponse>()
			.Map(dest => dest.Id, src => src.Id.Value);
	}
}
