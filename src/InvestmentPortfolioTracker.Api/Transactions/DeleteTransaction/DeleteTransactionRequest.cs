using System.Security.Claims;
using FastEndpoints;

namespace InvestmentPortfolioTracker.Api.Transactions;

internal sealed record DeleteTransactionRequest(
	[property: FromClaim(ClaimTypes.NameIdentifier)] Guid UserId,
	Guid AssetItemId,
	Guid TransactionId);
