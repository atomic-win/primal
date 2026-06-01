using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;
using Primal.Infrastructure.Investments;
using Primal.Infrastructure.Persistence;
using Primal.Infrastructure.Users;

namespace Primal.Infrastructure.IntegrationTests.Investments;

public sealed class AssetItemRepositoryTests
{
	[Test]
	public async Task GetAllAsync_Empty_ReturnsEmpty()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new AssetItemRepository(db, TimeProvider.System);
		var userId = await AddUserAsync(db);

		var result = await repository.GetAllAsync(userId, CancellationToken.None);

		await Assert.That(result.Count()).IsEqualTo(0);
	}

	[Test]
	public async Task AddAsync_ThenGetAllAsync_ReturnsItem()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new AssetItemRepository(db, TimeProvider.System);
		var userId = await AddUserAsync(db);
		var asset = await AddAssetAsync(db);

		var addedItem = await repository.AddAsync(userId, asset.Id, "Brokerage Account", CancellationToken.None);
		var result = (await repository.GetAllAsync(userId, CancellationToken.None)).ToArray();

		await Assert.That(result.Length).IsEqualTo(1);
		await Assert.That(result[0].Id == addedItem.Id).IsTrue();
		await Assert.That(result[0].AssetId == asset.Id).IsTrue();
		await Assert.That(string.Equals(result[0].Name, "Brokerage Account", StringComparison.Ordinal)).IsTrue();
	}

	[Test]
	public async Task GetByIdAsync_NonExistent_ReturnsEmptyAssetItem()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new AssetItemRepository(db, TimeProvider.System);
		var userId = await AddUserAsync(db);

		var result = await repository.GetByIdAsync(userId, new AssetItemId(Guid.NewGuid()), CancellationToken.None);

		await Assert.That(result.Id == AssetItemId.Empty).IsTrue();
		await Assert.That(result.AssetId == AssetId.Empty).IsTrue();
	}

	[Test]
	public async Task AddAsync_ThenGetByIdAsync_ReturnsCorrectItem()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new AssetItemRepository(db, TimeProvider.System);
		var userId = await AddUserAsync(db);
		var asset = await AddAssetAsync(db);

		var addedItem = await repository.AddAsync(userId, asset.Id, "Retirement Account", CancellationToken.None);
		var result = await repository.GetByIdAsync(userId, addedItem.Id, CancellationToken.None);

		await Assert.That(result.Id == addedItem.Id).IsTrue();
		await Assert.That(result.AssetId == asset.Id).IsTrue();
		await Assert.That(string.Equals(result.Name, "Retirement Account", StringComparison.Ordinal)).IsTrue();
	}

	[Test]
	public async Task DeleteAsync_ThenGetByIdAsync_ReturnsEmpty()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new AssetItemRepository(db, TimeProvider.System);
		var userId = await AddUserAsync(db);
		var asset = await AddAssetAsync(db);
		var addedItem = await repository.AddAsync(userId, asset.Id, "Disposable Account", CancellationToken.None);

		await repository.DeleteAsync(userId, addedItem.Id, CancellationToken.None);

		var result = await repository.GetByIdAsync(userId, addedItem.Id, CancellationToken.None);

		await Assert.That(result.Id == AssetItemId.Empty).IsTrue();
	}

	private static async Task<UserId> AddUserAsync(DbConnectionFactory db)
	{
		var repository = new UserRepository(db, TimeProvider.System);
		var userId = new UserId(Guid.NewGuid());
		await repository.AddUserAsync(
			userId,
			"grace@example.com",
			"Grace",
			"Hopper",
			"Grace Hopper",
			CancellationToken.None);
		return userId;
	}

	private static Task<Asset> AddAssetAsync(DbConnectionFactory db)
	{
		var repository = new AssetRepository(db, TimeProvider.System);
		return repository.AddAsync(
			"Index Fund",
			AssetClass.Equity,
			AssetType.MutualFund,
			Currency.USD,
			"asset-123",
			CancellationToken.None);
	}
}
