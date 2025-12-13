namespace Primal.Infrastructure.Persistence;

internal sealed class UserTableEntity : TableEntity
{
	required internal Guid Id { get; init; }

	required internal string Email { get; init; } = null!;

	required internal string FirstName { get; init; } = null!;

	required internal string LastName { get; init; } = null!;

	required internal string FullName { get; init; } = null!;
}
