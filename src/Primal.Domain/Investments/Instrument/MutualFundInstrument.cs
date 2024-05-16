namespace Primal.Domain.Investments;

public sealed class MutualFundInstrument : InvestmentInstrument
{
	public MutualFundInstrument(InstrumentId id, string name, InvestmentCategory category, MutualFundId mutualFundId)
		: base(id, name, category, InvestmentType.MutualFunds)
	{
		this.MutualFundId = mutualFundId;
	}

	public MutualFundId MutualFundId { get; init; }
}
