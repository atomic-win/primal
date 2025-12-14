using Primal.Domain.Money;

namespace Primal.Application.Investments;

public sealed record MutualFund(
	string SchemeCode,
	string Name,
	string SchemeType,
	string SchemeCategory,
	Currency Currency);
