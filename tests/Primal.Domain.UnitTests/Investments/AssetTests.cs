using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Domain.UnitTests.Investments;

public sealed class AssetTests
{
	[Test]
	public async Task Empty_ReturnsExpectedDefaultValues()
	{
		var asset = Asset.Empty;

		await Assert.That(asset.Id == AssetId.Empty).IsTrue();
		await Assert.That(string.Equals(asset.Name, string.Empty, StringComparison.Ordinal)).IsTrue();
		await Assert.That(asset.AssetClass == AssetClass.Unknown).IsTrue();
		await Assert.That(asset.AssetType == AssetType.Unknown).IsTrue();
		await Assert.That(asset.Currency == Currency.Unknown).IsTrue();
		await Assert.That(string.Equals(asset.ExternalId, string.Empty, StringComparison.Ordinal)).IsTrue();
	}

	[Test]
	public async Task Constructor_SetsAllProperties()
	{
		var id = new AssetId(Guid.NewGuid());
		var asset = new Asset(
			id,
			"Global Equity Fund",
			AssetClass.Equity,
			AssetType.MutualFund,
			Currency.USD,
			"mf-12345");

		await Assert.That(asset.Id == id).IsTrue();
		await Assert.That(string.Equals(asset.Name, "Global Equity Fund", StringComparison.Ordinal)).IsTrue();
		await Assert.That(asset.AssetClass == AssetClass.Equity).IsTrue();
		await Assert.That(asset.AssetType == AssetType.MutualFund).IsTrue();
		await Assert.That(asset.Currency == Currency.USD).IsTrue();
		await Assert.That(string.Equals(asset.ExternalId, "mf-12345", StringComparison.Ordinal)).IsTrue();
	}
}
