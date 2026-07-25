using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Money;

namespace InvestmentPortfolioTracker.Core.Investments;

public sealed record Stock(
	string Symbol,
	string Name,
	AssetType AssetType,
	Currency Currency);
