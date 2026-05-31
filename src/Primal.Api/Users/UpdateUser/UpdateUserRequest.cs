using System.Security.Claims;
using FastEndpoints;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.Users;

internal sealed record UpdateUserRequest(
	Currency PreferredCurrency,
	Locale PreferredLocale)
{
	[FromClaim(ClaimTypes.NameIdentifier)]
	public Guid UserId { get; set; }
}
