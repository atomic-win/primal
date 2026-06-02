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

		await Verifier.Verify(new
		{
			EqualityOperator = left == right,
			EqualsTyped = left.Equals(right),
			EqualsObject = left.Equals((object)right),
		});
	}

	[Test]
	public async Task TwoEntitiesWithDifferentIds_AreNotEqual()
	{
		var left = CreateAsset(new AssetId(Guid.NewGuid()));
		var right = CreateAsset(new AssetId(Guid.NewGuid()));

		await Verifier.Verify(new
		{
			EqualityOperator = left == right,
			EqualsTyped = left.Equals(right),
			EqualsObject = left.Equals((object)right),
		});
	}

	[Test]
	public async Task InequalityOperator_ReturnsTrue_ForDifferentIds()
	{
		var left = CreateAsset(new AssetId(Guid.NewGuid()));
		var right = CreateAsset(new AssetId(Guid.NewGuid()));

		await Verifier.Verify(left != right);
	}

	[Test]
	public async Task GetHashCode_ReturnsSameValue_ForSameId()
	{
		var id = new AssetId(Guid.NewGuid());
		var left = CreateAsset(id, "Asset One", AssetType.MutualFund, "mf-12345");
		var right = CreateAsset(id, "Asset Two", AssetType.Stock, "stock-AAPL");

		await Verifier.Verify(left.GetHashCode() == right.GetHashCode());
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

		await Verifier.Verify(left.GetHashCode() != right.GetHashCode());
	}

	[Test]
	public async Task Equals_ReturnsFalse_ForNull()
	{
		var asset = CreateAsset(new AssetId(Guid.NewGuid()));

		await Verifier.Verify(asset.Equals(null));
	}

	[Test]
	public async Task Equals_Object_ReturnsFalse_ForNull()
	{
		var asset = CreateAsset(new AssetId(Guid.NewGuid()));

		await Verifier.Verify(asset.Equals((object)null!));
	}

	[Test]
	public async Task EqualityOperator_ReturnsTrue_ForBothNull()
	{
		Asset left = null;
		Asset right = null;

		await Verifier.Verify(left == right);
	}

	[Test]
	public async Task EqualityOperator_ReturnsFalse_WhenLeftNull()
	{
		Asset left = null;
		var right = CreateAsset(new AssetId(Guid.NewGuid()));

		await Verifier.Verify(left == right);
	}

	[Test]
	public async Task EqualityOperator_ReturnsFalse_WhenRightNull()
	{
		var left = CreateAsset(new AssetId(Guid.NewGuid()));
		Asset right = null;

		await Verifier.Verify(left == right);
	}

	[Test]
	public async Task InequalityOperator_ReturnsFalse_ForSameId()
	{
		var id = new AssetId(Guid.NewGuid());
		var left = CreateAsset(id);
		var right = CreateAsset(id);

		await Verifier.Verify(left != right);
	}

	[Test]
	public async Task Equals_ReturnsFalse_ForDifferentType()
	{
		var asset = CreateAsset(new AssetId(Guid.NewGuid()));
		object other = new AssetItem(new AssetItemId(Guid.NewGuid()), new AssetId(Guid.NewGuid()), "Item");

		await Verifier.Verify(asset.Equals(other));
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
