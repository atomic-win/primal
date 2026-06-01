using Primal.Domain.Investments;

namespace Primal.Domain.UnitTests.Investments;

public sealed class AssetItemTests
{
	[Test]
	public async Task Empty_ReturnsExpectedDefaultValues()
	{
		var assetItem = AssetItem.Empty;

		await Verifier.Verify(assetItem);
	}

	[Test]
	public async Task Constructor_SetsAllProperties()
	{
		var id = new AssetItemId(Guid.NewGuid());
		var assetId = new AssetId(Guid.NewGuid());
		var assetItem = new AssetItem(id, assetId, "Primary Account");

		await Verifier.Verify(assetItem);
	}
}
