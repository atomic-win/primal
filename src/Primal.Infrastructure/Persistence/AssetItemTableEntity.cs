namespace Primal.Infrastructure.Persistence;

internal sealed class AssetItemTableEntity : TableEntity
{
	internal required Guid Id { get; set; }

	internal required string Name { get; set; }

	internal required Guid UserId { get; set; }

	internal required Guid AssetId { get; set; }
}
