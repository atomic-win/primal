using InvestmentPortfolioTracker.Domain.Investments;

namespace InvestmentPortfolioTracker.Api.Transactions;

internal sealed record TransactionResponse(
	Guid Id,
	DateOnly Date,
	string Name,
	TransactionType TransactionType,
	Guid AssetItemId,
	decimal Units,
	decimal Price,
	decimal Amount);
