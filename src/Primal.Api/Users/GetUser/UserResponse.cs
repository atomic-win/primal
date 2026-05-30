using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.Users;

internal sealed record UserResponse(
	Guid Id,
	string Email,
	string FirstName,
	string LastName,
	string FullName,
	Currency PreferredCurrency,
	Locale PreferredLocale);
