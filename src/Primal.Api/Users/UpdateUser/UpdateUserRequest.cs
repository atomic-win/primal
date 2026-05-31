using System.Security.Claims;
using FastEndpoints;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.Users;

internal sealed record UpdateUserRequest(
	[property: FromClaim(ClaimTypes.NameIdentifier)] Guid UserId,
	Currency PreferredCurrency,
	Locale PreferredLocale);
