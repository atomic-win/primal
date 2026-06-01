using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Domain.UnitTests.Investments;

public sealed class AssetTests
{
	[Test]
	public async Task Empty_ReturnsExpectedDefaultValues()
	{
		var asset = Asset.Empty;

		await Verifier.Verify(asset);
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

		await Verifier.Verify(asset);
	}
}
