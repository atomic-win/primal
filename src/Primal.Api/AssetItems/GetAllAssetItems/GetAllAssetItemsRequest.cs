using System.Security.Claims;
using FastEndpoints;

namespace Primal.Api.AssetItems;

internal sealed record GetAllAssetItemsRequest(
	[property: FromClaim(ClaimTypes.NameIdentifier)] Guid UserId);
