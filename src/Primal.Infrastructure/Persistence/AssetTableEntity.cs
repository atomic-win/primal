namespace Primal.Infrastructure.Persistence;

internal sealed class AssetTableEntity : TableEntity
{
	public string Id { get; set; } = null!;

	public string Name { get; set; } = null!;

	public string AssetClass { get; set; } = null!;

	public string AssetType { get; set; } = null!;

	public string Currency { get; set; } = null!;

	public string ExternalId { get; set; } = null!;
}
