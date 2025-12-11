namespace Primal.Infrastructure.Persistence;

internal abstract class TableEntity
{
	internal DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

	internal DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
