using Primal.Domain.Common.Models;

namespace Primal.Domain.Investments;

public sealed class AssetItem : Entity<AssetItemId>
{
	public AssetItem(
		AssetItemId id,
		AssetId assetId,
		string name)
		: base(id)
	{
		this.AssetId = assetId;
		this.Name = name;
	}

	public static AssetItem Empty { get; } = new AssetItem(
		AssetItemId.Empty,
		AssetId.Empty,
		string.Empty);

	public AssetId AssetId { get; init; }

	public string Name { get; init; }
}
