using System.Security.Claims;
using FastEndpoints;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Api.AssetItems;

internal sealed record AddAssetItemRequest(
	[property: FromClaim(ClaimTypes.NameIdentifier)] Guid UserId,
	string Name,
	AssetClass AssetClass,
	AssetType AssetType,
	string ExternalId,
	Currency Currency);
