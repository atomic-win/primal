namespace Primal.Infrastructure.Persistence;

internal sealed class AssetItemTableEntity : TableEntity
{
	public string Id { get; set; } = null!;

	public string Name { get; set; } = null!;

	public string UserId { get; set; } = null!;

	public string AssetId { get; set; } = null!;
}
