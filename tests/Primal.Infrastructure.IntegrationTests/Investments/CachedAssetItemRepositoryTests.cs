using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;
using Primal.Infrastructure.Investments;
using Primal.Infrastructure.Users;

namespace Primal.Infrastructure.IntegrationTests.Investments;

public sealed class CachedAssetItemRepositoryTests
{
	[Test]
	public async Task GetAll_SecondCall_ReturnsFromCache()
	{
		var (cached, userId, assetId) = await CreateTestContext();

		await cached.AddAsync(userId, assetId, "My Fund", CancellationToken.None);

		var first = await cached.GetAllAsync(userId, CancellationToken.None);
		var second = await cached.GetAllAsync(userId, CancellationToken.None);

		await Assert.That(first.Count()).IsEqualTo(1);
		await Assert.That(second.Count()).IsEqualTo(1);
	}

	[Test]
	public async Task Add_InvalidatesListCache()
	{
		var (cached, userId, assetId) = await CreateTestContext();

		// Populate cache with empty list
		await cached.GetAllAsync(userId, CancellationToken.None);

		// Add item
		await cached.AddAsync(userId, assetId, "My Fund", CancellationToken.None);

		// GetAll should return updated list
		var result = await cached.GetAllAsync(userId, CancellationToken.None);

		await Assert.That(result.Count()).IsEqualTo(1);
	}

	[Test]
	public async Task Delete_InvalidatesListAndItemCache()
	{
		var (cached, userId, assetId) = await CreateTestContext();

		var item = await cached.AddAsync(userId, assetId, "My Fund", CancellationToken.None);

		// Populate caches
		await cached.GetAllAsync(userId, CancellationToken.None);
		await cached.GetByIdAsync(userId, item.Id, CancellationToken.None);

		// Delete
		await cached.DeleteAsync(userId, item.Id, CancellationToken.None);

		var allResult = await cached.GetAllAsync(userId, CancellationToken.None);
		var byIdResult = await cached.GetByIdAsync(userId, item.Id, CancellationToken.None);

		await Assert.That(allResult.Count()).IsEqualTo(0);
		await Assert.That(byIdResult.Id).IsEqualTo(AssetItemId.Empty);
	}

	private static async Task<(CachedAssetItemRepository Cached, UserId UserId, AssetId AssetId)> CreateTestContext()
	{
		var cache = TestCacheHelper.CreateHybridCache();
		var db = TestDbHelper.CreateTestDatabase();
		var userRepo = new UserRepository(db, TimeProvider.System);
		var assetRepo = new AssetRepository(db, TimeProvider.System);
		var inner = new AssetItemRepository(db, TimeProvider.System);
		var cached = new CachedAssetItemRepository(cache, inner);

		var user = await userRepo.AddUserAsync("test@example.com", "Test", "User", "Test User", CancellationToken.None);
		var asset = await assetRepo.AddAsync("Test", AssetClass.Equity, AssetType.MutualFund, Currency.INR, "mf-123", CancellationToken.None);

		return (cached, user.Id, asset.Id);
	}
}
