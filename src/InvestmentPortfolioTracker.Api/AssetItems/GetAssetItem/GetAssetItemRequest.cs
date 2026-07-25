using System.Security.Claims;

using FastEndpoints;

namespace InvestmentPortfolioTracker.Api.AssetItems;

internal sealed record GetAssetItemRequest(
	[property: FromClaim(ClaimTypes.NameIdentifier)] Guid UserId,
	Guid Id);
