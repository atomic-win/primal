using Mapster;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class InvestmentMappingConfig : IRegister
{
	public void Register(TypeAdapterConfig config)
	{
		config.NewConfig<Instrument, InstrumentResult>()
			.Include<MutualFundInstrument, MutualFundInstrumentResult>();
	}
}
