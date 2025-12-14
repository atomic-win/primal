using Primal.Domain.Users;

namespace Primal.Infrastructure.Persistence;

internal sealed class UserIdTableEntity : TableEntity
{
	required internal string Id { get; init; } = null!;

	required internal IdentityProvider IdentityProvider { get; init; }

	required internal Guid UserId { get; init; }
}
