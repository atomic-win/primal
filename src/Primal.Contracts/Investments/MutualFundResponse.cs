namespace Primal.Contracts.Investments;

public sealed record MutualFundResponse(
	Guid Id,
	string SchemeName,
	string FundHouse,
	string SchemeType,
	string SchemeCategory,
	int SchemeCode,
	string Currency);
