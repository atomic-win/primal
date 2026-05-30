using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Domain.UnitTests.Common;

public sealed class EntityTests
{
	[Test]
	public async Task TwoEntitiesWithSameId_AreEqual()
	{
		var id = new AssetId(Guid.NewGuid());
		var left = CreateAsset(id, "Asset One", AssetType.MutualFund, "mf-12345");
		var right = CreateAsset(id, "Asset Two", AssetType.Stock, "stock-AAPL");

		await Assert.That(left == right).IsTrue();
		await Assert.That(left.Equals(right)).IsTrue();
		await Assert.That(left.Equals((object)right)).IsTrue();
	}

	[Test]
	public async Task TwoEntitiesWithDifferentIds_AreNotEqual()
	{
		var left = CreateAsset(new AssetId(Guid.NewGuid()));
		var right = CreateAsset(new AssetId(Guid.NewGuid()));

		await Assert.That(left == right).IsFalse();
		await Assert.That(left.Equals(right)).IsFalse();
		await Assert.That(left.Equals((object)right)).IsFalse();
	}

	[Test]
	public async Task InequalityOperator_ReturnsTrue_ForDifferentIds()
	{
		var left = CreateAsset(new AssetId(Guid.NewGuid()));
		var right = CreateAsset(new AssetId(Guid.NewGuid()));

		await Assert.That(left != right).IsTrue();
	}

	[Test]
	public async Task GetHashCode_ReturnsSameValue_ForSameId()
	{
		var id = new AssetId(Guid.NewGuid());
		var left = CreateAsset(id, "Asset One", AssetType.MutualFund, "mf-12345");
		var right = CreateAsset(id, "Asset Two", AssetType.Stock, "stock-AAPL");

		await Assert.That(left.GetHashCode() == right.GetHashCode()).IsTrue();
	}

	[Test]
	public async Task GetHashCode_ReturnsDifferentValue_ForDifferentIds()
	{
		var firstGuid = Guid.NewGuid();
		var secondGuid = Guid.NewGuid();

		while (secondGuid.GetHashCode() == firstGuid.GetHashCode())
		{
			secondGuid = Guid.NewGuid();
		}

		var left = CreateAsset(new AssetId(firstGuid));
		var right = CreateAsset(new AssetId(secondGuid));

		await Assert.That(left.GetHashCode() != right.GetHashCode()).IsTrue();
	}

	[Test]
	public async Task Equals_ReturnsFalse_ForNull()
	{
		var asset = CreateAsset(new AssetId(Guid.NewGuid()));

		await Assert.That(asset.Equals(null)).IsFalse();
	}

	[Test]
	public async Task Equals_ReturnsFalse_ForDifferentType()
	{
		var asset = CreateAsset(new AssetId(Guid.NewGuid()));
		object other = new AssetItem(new AssetItemId(Guid.NewGuid()), new AssetId(Guid.NewGuid()), "Item");

		await Assert.That(asset.Equals(other)).IsFalse();
	}

	private static Asset CreateAsset(
		AssetId id,
		string name = "Test Asset",
		AssetType assetType = AssetType.Stock,
		string externalId = "stock-TEST")
	{
		return new Asset(
			id,
			name,
			AssetClass.Equity,
			assetType,
			Currency.USD,
			externalId);
	}
}
