using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Money;
using InvestmentPortfolioTracker.Domain.Users;
using InvestmentPortfolioTracker.Infrastructure.Investments;
using InvestmentPortfolioTracker.Infrastructure.Users;

namespace InvestmentPortfolioTracker.Infrastructure.IntegrationTests.Investments;

public sealed class CachedTransactionRepositoryTests
{
	[Test]
	public async Task GetByAssetItemId_SecondCall_ReturnsFromCache()
	{
		var (cached, userId, assetItemId) = await CreateTestContext();

		await cached.AddAsync(userId, assetItemId, new DateOnly(2026, 1, 15), "Buy", TransactionType.Buy, 10, 100, 0, CancellationToken.None);

		var first = await cached.GetByAssetItemIdAsync(userId, assetItemId, CancellationToken.None);
		var second = await cached.GetByAssetItemIdAsync(userId, assetItemId, CancellationToken.None);

		await Verifier.Verify(new { first, second });
	}

	[Test]
	public async Task Add_InvalidatesCache()
	{
		var (cached, userId, assetItemId) = await CreateTestContext();

		// Populate cache with empty list
		await cached.GetByAssetItemIdAsync(userId, assetItemId, CancellationToken.None);

		// Add transaction
		await cached.AddAsync(userId, assetItemId, new DateOnly(2026, 1, 15), "Buy", TransactionType.Buy, 10, 100, 0, CancellationToken.None);

		var result = await cached.GetByAssetItemIdAsync(userId, assetItemId, CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task Update_InvalidatesCache()
	{
		var (cached, userId, assetItemId) = await CreateTestContext();

		var txn = await cached.AddAsync(userId, assetItemId, new DateOnly(2026, 1, 15), "Buy", TransactionType.Buy, 10, 100, 0, CancellationToken.None);

		// Populate cache
		await cached.GetByAssetItemIdAsync(userId, assetItemId, CancellationToken.None);

		// Update
		var updated = new Transaction(txn.Id, txn.Date, "Updated", txn.TransactionType, assetItemId, 20, 200, 0);
		await cached.UpdateAsync(userId, updated, CancellationToken.None);

		var result = await cached.GetByIdAsync(userId, assetItemId, txn.Id, CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task Delete_InvalidatesCache()
	{
		var (cached, userId, assetItemId) = await CreateTestContext();

		var txn = await cached.AddAsync(userId, assetItemId, new DateOnly(2026, 1, 15), "Buy", TransactionType.Buy, 10, 100, 0, CancellationToken.None);

		// Populate cache
		await cached.GetByAssetItemIdAsync(userId, assetItemId, CancellationToken.None);

		// Delete
		await cached.DeleteAsync(userId, assetItemId, txn.Id, CancellationToken.None);

		var result = await cached.GetByAssetItemIdAsync(userId, assetItemId, CancellationToken.None);

		await Verifier.Verify(result);
	}

	private static async Task<(CachedTransactionRepository Cached, UserId UserId, AssetItemId AssetItemId)> CreateTestContext()
	{
		var cache = TestCacheFactory.CreateHybridCache();
		var db = TestDbFactory.CreateTestDatabase();
		var userRepo = new UserRepository(db, TimeProvider.System);
		var assetRepo = new AssetRepository(db, TimeProvider.System);
		var assetItemRepo = new AssetItemRepository(db, TimeProvider.System);
		var inner = new TransactionRepository(db, TimeProvider.System);
		var cached = new CachedTransactionRepository(cache, TimeProvider.System, inner);

		var user = await userRepo.AddUserAsync("test@example.com", "Test", "User", "Test User", CancellationToken.None);
		var asset = await assetRepo.AddAsync("Test", AssetClass.Equity, AssetType.MutualFund, Currency.INR, "mf-123", CancellationToken.None);
		var item = await assetItemRepo.AddAsync(user.Id, asset.Id, "My Fund", CancellationToken.None);

		return (cached, user.Id, item.Id);
	}
}
