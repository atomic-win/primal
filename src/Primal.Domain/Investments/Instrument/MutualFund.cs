using Primal.Domain.Money;

namespace Primal.Domain.Investments;

public sealed class MutualFund : InvestmentInstrument
{
	public MutualFund(
		InstrumentId id,
		string name,
		string fundHouse,
		string schemeType,
		string schemeCategory,
		int schemeCode,
		Currency currency)
		: base(id, name, InstrumentType.MutualFunds, currency)
	{
		this.FundHouse = fundHouse;
		this.SchemeType = schemeType;
		this.SchemeCategory = schemeCategory;
		this.SchemeCode = schemeCode;
	}

	public string FundHouse { get; init; }

	public string SchemeType { get; init; }

	public string SchemeCategory { get; init; }

	public int SchemeCode { get; init; }
}
