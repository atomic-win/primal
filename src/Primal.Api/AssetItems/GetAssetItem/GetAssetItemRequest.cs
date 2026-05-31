using System.Security.Claims;
using FastEndpoints;

namespace Primal.Api.AssetItems;

internal sealed record GetAssetItemRequest(
	[property: FromClaim(ClaimTypes.NameIdentifier)] Guid UserId,
	Guid Id);
