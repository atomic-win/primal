using InvestmentPortfolioTracker.Domain.Money;
using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Api.Users;

internal sealed record UserResponse(
	Guid Id,
	string Email,
	string FirstName,
	string LastName,
	string FullName,
	Currency PreferredCurrency,
	Locale PreferredLocale);
