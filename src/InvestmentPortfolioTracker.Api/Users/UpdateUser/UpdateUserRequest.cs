using System.Security.Claims;

using FastEndpoints;

using InvestmentPortfolioTracker.Domain.Money;
using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Api.Users;

internal sealed record UpdateUserRequest(
	[property: FromClaim(ClaimTypes.NameIdentifier)] Guid UserId,
	Currency PreferredCurrency,
	Locale PreferredLocale);
