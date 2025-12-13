namespace Primal.Infrastructure.Persistence;

internal abstract class TableEntity
{
	internal DateTimeOffset CreatedAt { get; set; }

	internal DateTimeOffset UpdatedAt { get; set; }
}
