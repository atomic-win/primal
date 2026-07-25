using System.Security.Claims;
using FastEndpoints;
using InvestmentPortfolioTracker.Domain.Money;

namespace InvestmentPortfolioTracker.Api.Transactions;

internal sealed record GetAllByAssetItemIdRequest(
	[property: FromClaim(ClaimTypes.NameIdentifier)] Guid UserId,
	Guid AssetItemId,
	Currency Currency);
