using Primal.Domain.Common.Models;
using Primal.Domain.Money;

namespace Primal.Domain.Investments;

public sealed class MutualFund : Entity<MutualFundId>
{
	public MutualFund(MutualFundId id, string schemeName, string fundHouse, string schemeType, string schemeCategory, int schemeCode, Currency currency)
		: base(id)
	{
		this.SchemeName = schemeName;
		this.FundHouse = fundHouse;
		this.SchemeType = schemeType;
		this.SchemeCategory = schemeCategory;
		this.SchemeCode = schemeCode;
		this.Currency = currency;
	}

	public string SchemeName { get; init; }

	public string FundHouse { get; init; }

	public string SchemeType { get; init; }

	public string SchemeCategory { get; init; }

	public int SchemeCode { get; init; }

	public Currency Currency { get; init; }
}
