using Primal.Domain.Investments;

namespace Primal.Domain.UnitTests.Investments;

public sealed class AssetItemTests
{
	[Test]
	public async Task Empty_ReturnsExpectedDefaultValues()
	{
		var assetItem = AssetItem.Empty;

		await Assert.That(assetItem.Id == AssetItemId.Empty).IsTrue();
		await Assert.That(assetItem.AssetId == AssetId.Empty).IsTrue();
		await Assert.That(string.Equals(assetItem.Name, string.Empty, StringComparison.Ordinal)).IsTrue();
	}

	[Test]
	public async Task Constructor_SetsAllProperties()
	{
		var id = new AssetItemId(Guid.NewGuid());
		var assetId = new AssetId(Guid.NewGuid());
		var assetItem = new AssetItem(id, assetId, "Primary Account");

		await Assert.That(assetItem.Id == id).IsTrue();
		await Assert.That(assetItem.AssetId == assetId).IsTrue();
		await Assert.That(string.Equals(assetItem.Name, "Primary Account", StringComparison.Ordinal)).IsTrue();
	}
}
