namespace Primal.Infrastructure.Persistence;

internal sealed class UserIdTableEntity : TableEntity
{
	public string Id { get; set; } = null!;

	public string IdentityProvider { get; set; } = null!;

	public string UserId { get; set; } = null!;
}
