namespace Primal.Contracts.Investments;

public sealed record MutualFundResponse(
	Guid Id,
	string Name,
	string Type,
	string FundHouse,
	string SchemeType,
	string SchemeCategory,
	int SchemeCode,
	string Currency) : InstrumentResponse(Id, Name, Type, Currency);
