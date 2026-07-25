using System.Security.Claims;

using FastEndpoints;

using InvestmentPortfolioTracker.Domain.Money;

namespace InvestmentPortfolioTracker.Api.AssetItems;

internal sealed record GetValuationsRequest(
	[property: FromClaim(ClaimTypes.NameIdentifier)] Guid UserId,
	IEnumerable<Guid> AssetItemIds,
	Currency Currency);
