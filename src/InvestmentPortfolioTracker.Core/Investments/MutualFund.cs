using InvestmentPortfolioTracker.Domain.Money;

namespace InvestmentPortfolioTracker.Core.Investments;

public sealed record MutualFund(
	string SchemeCode,
	string Name,
	string SchemeType,
	string SchemeCategory,
	Currency Currency);
