using System.Security.Claims;
using FastEndpoints;
using InvestmentPortfolioTracker.Domain.Investments;

namespace InvestmentPortfolioTracker.Api.Transactions;

internal sealed record EditTransactionRequest(
	[property: FromClaim(ClaimTypes.NameIdentifier)] Guid UserId,
	Guid AssetItemId,
	Guid TransactionId,
	string Name,
	TransactionType TransactionType,
	decimal Units,
	decimal Price,
	decimal Amount);
