using System.Security.Claims;
using FastEndpoints;

namespace Primal.Api.Users;

internal sealed record GetUserRequest(
	[property: FromClaim(ClaimTypes.NameIdentifier)] Guid UserId);
