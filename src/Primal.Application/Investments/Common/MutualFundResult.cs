using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Application.Investments;

public sealed record MutualFundResult(
	MutualFundId Id,
	string SchemeName,
	string FundHouse,
	string SchemeType,
	string SchemeCategory,
	int SchemeCode,
	Currency Currency);
