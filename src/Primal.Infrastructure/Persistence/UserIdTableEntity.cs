using Primal.Domain.Users;

namespace Primal.Infrastructure.Persistence;

internal sealed class UserIdTableEntity : TableEntity
{
	internal required string Id { get; init; } = null!;

	internal required IdentityProvider IdentityProvider { get; init; }

	internal required Guid UserId { get; init; }
}
