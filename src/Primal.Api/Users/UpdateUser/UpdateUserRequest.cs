using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.Users;

internal sealed record UpdateUserRequest(
	Currency PreferredCurrency,
	Locale PreferredLocale);
