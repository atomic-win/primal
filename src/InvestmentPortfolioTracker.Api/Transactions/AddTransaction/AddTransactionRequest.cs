using System.Security.Claims;
using FastEndpoints;
using InvestmentPortfolioTracker.Domain.Investments;

namespace InvestmentPortfolioTracker.Api.Transactions;

internal sealed record AddTransactionRequest(
	[property: FromClaim(ClaimTypes.NameIdentifier)] Guid UserId,
	Guid AssetItemId,
	DateOnly Date,
	string Name,
	TransactionType TransactionType,
	decimal Units,
	decimal Price,
	decimal Amount);
