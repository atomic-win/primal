using System.Security.Claims;
using FastEndpoints;
using Primal.Domain.Money;

namespace Primal.Api.Transactions;

internal sealed record GetAllByAssetItemIdRequest(
	[property: FromClaim(ClaimTypes.NameIdentifier)] Guid UserId,
	Guid AssetItemId,
	Currency Currency);
