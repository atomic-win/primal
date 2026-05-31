using System.Security.Claims;
using FastEndpoints;
using Primal.Domain.Money;

namespace Primal.Api.AssetItems;

internal sealed record GetValuationsRequest(
	[property: FromClaim(ClaimTypes.NameIdentifier)] Guid UserId,
	IEnumerable<Guid> AssetItemIds,
	Currency Currency);
