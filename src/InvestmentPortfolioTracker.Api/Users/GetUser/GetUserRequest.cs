using System.Security.Claims;

using FastEndpoints;

namespace InvestmentPortfolioTracker.Api.Users;

internal sealed record GetUserRequest(
	[property: FromClaim(ClaimTypes.NameIdentifier)] Guid UserId);
