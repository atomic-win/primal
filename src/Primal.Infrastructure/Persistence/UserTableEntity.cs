using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Persistence;

internal sealed class UserTableEntity : TableEntity
{
	internal required Guid Id { get; init; }

	internal required string Email { get; init; } = null!;

	internal required string FirstName { get; init; } = null!;

	internal required string LastName { get; init; } = null!;

	internal required string FullName { get; init; } = null!;

	internal required Currency PreferredCurrency { get; init; }

	internal required Locale PreferredLocale { get; init; }
}
