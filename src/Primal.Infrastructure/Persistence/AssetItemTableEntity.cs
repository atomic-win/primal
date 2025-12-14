namespace Primal.Infrastructure.Persistence;

internal sealed class AssetItemTableEntity : TableEntity
{
	required internal Guid Id { get; set; }

	required internal string Name { get; set; }

	required internal Guid UserId { get; set; }

	required internal Guid AssetId { get; set; }
}
