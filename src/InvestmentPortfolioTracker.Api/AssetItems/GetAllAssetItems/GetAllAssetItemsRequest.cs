using System.Security.Claims;
using FastEndpoints;

namespace InvestmentPortfolioTracker.Api.AssetItems;

internal sealed record GetAllAssetItemsRequest(
	[property: FromClaim(ClaimTypes.NameIdentifier)] Guid UserId);
