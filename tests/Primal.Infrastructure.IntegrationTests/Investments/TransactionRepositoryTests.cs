using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;
using Primal.Infrastructure.Investments;
using Primal.Infrastructure.Persistence;
using Primal.Infrastructure.Users;

namespace Primal.Infrastructure.IntegrationTests.Investments;

public sealed class TransactionRepositoryTests
{
	[Test]
	public async Task GetByAssetItemIdAsync_Empty_ReturnsEmpty()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new TransactionRepository(db, TimeProvider.System);
		var userId = await AddUserAsync(db);
		var asset = await AddAssetAsync(db);
		var assetItem = await AddAssetItemAsync(db, userId, asset.Id);

		var result = await repository.GetByAssetItemIdAsync(userId, assetItem.Id, CancellationToken.None);

		await Assert.That(result.Count()).IsEqualTo(0);
	}

	[Test]
	public async Task AddAsync_ThenGetByAssetItemIdAsync_ReturnsTransaction()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new TransactionRepository(db, TimeProvider.System);
		var userId = await AddUserAsync(db);
		var asset = await AddAssetAsync(db);
		var assetItem = await AddAssetItemAsync(db, userId, asset.Id);
		var date = new DateOnly(2024, 6, 15);

		var addedTransaction = await repository.AddAsync(
			userId,
			assetItem.Id,
			date,
			"Monthly Investment",
			TransactionType.Buy,
			10.5m,
			25.75m,
			270.375m,
			CancellationToken.None);

		var result = (await repository.GetByAssetItemIdAsync(userId, assetItem.Id, CancellationToken.None)).ToArray();

		await Assert.That(result.Length).IsEqualTo(1);
		await Assert.That(result[0].Id == addedTransaction.Id).IsTrue();
		await Assert.That(result[0].Date == date).IsTrue();
		await Assert.That(string.Equals(result[0].Name, "Monthly Investment", StringComparison.Ordinal)).IsTrue();
		await Assert.That(result[0].TransactionType == TransactionType.Buy).IsTrue();
		await Assert.That(result[0].AssetItemId == assetItem.Id).IsTrue();
		await Assert.That(result[0].Units).IsEqualTo(10.5m);
		await Assert.That(result[0].Price).IsEqualTo(25.75m);
		await Assert.That(result[0].Amount).IsEqualTo(270.375m);
	}

	[Test]
	public async Task GetByIdAsync_NonExistent_ReturnsEmptyTransaction()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new TransactionRepository(db, TimeProvider.System);
		var userId = await AddUserAsync(db);
		var asset = await AddAssetAsync(db);
		var assetItem = await AddAssetItemAsync(db, userId, asset.Id);

		var result = await repository.GetByIdAsync(userId, assetItem.Id, new TransactionId(Guid.NewGuid()), CancellationToken.None);

		await Assert.That(result.Id == TransactionId.Empty).IsTrue();
		await Assert.That(result.AssetItemId == AssetItemId.Empty).IsTrue();
	}

	[Test]
	public async Task AddAsync_ThenGetByIdAsync_ReturnsCorrectTransaction()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new TransactionRepository(db, TimeProvider.System);
		var userId = await AddUserAsync(db);
		var asset = await AddAssetAsync(db);
		var assetItem = await AddAssetItemAsync(db, userId, asset.Id);
		var date = new DateOnly(2024, 7, 1);

		var addedTransaction = await repository.AddAsync(
			userId,
			assetItem.Id,
			date,
			"Dividend Reinvestment",
			TransactionType.Dividend,
			2m,
			100m,
			200m,
			CancellationToken.None);

		var result = await repository.GetByIdAsync(userId, assetItem.Id, addedTransaction.Id, CancellationToken.None);

		await Assert.That(result.Id == addedTransaction.Id).IsTrue();
		await Assert.That(result.Date == date).IsTrue();
		await Assert.That(string.Equals(result.Name, "Dividend Reinvestment", StringComparison.Ordinal)).IsTrue();
		await Assert.That(result.TransactionType == TransactionType.Dividend).IsTrue();
		await Assert.That(result.AssetItemId == assetItem.Id).IsTrue();
		await Assert.That(result.Units).IsEqualTo(2m);
		await Assert.That(result.Price).IsEqualTo(100m);
		await Assert.That(result.Amount).IsEqualTo(200m);
	}

	[Test]
	public async Task UpdateAsync_ThenGetByIdAsync_ReturnsUpdatedTransaction()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new TransactionRepository(db, TimeProvider.System);
		var userId = await AddUserAsync(db);
		var asset = await AddAssetAsync(db);
		var assetItem = await AddAssetItemAsync(db, userId, asset.Id);

		var addedTransaction = await repository.AddAsync(
			userId,
			assetItem.Id,
			new DateOnly(2024, 5, 10),
			"Initial Purchase",
			TransactionType.Buy,
			5m,
			20m,
			100m,
			CancellationToken.None);

		var updatedTransaction = new Transaction(
			addedTransaction.Id,
			new DateOnly(2024, 5, 12),
			"Updated Purchase",
			TransactionType.Sell,
			assetItem.Id,
			4m,
			30m,
			120m);

		await repository.UpdateAsync(userId, updatedTransaction, CancellationToken.None);

		var result = await repository.GetByIdAsync(userId, assetItem.Id, addedTransaction.Id, CancellationToken.None);

		await Assert.That(result.Id == addedTransaction.Id).IsTrue();
		await Assert.That(result.Date == new DateOnly(2024, 5, 12)).IsTrue();
		await Assert.That(string.Equals(result.Name, "Updated Purchase", StringComparison.Ordinal)).IsTrue();
		await Assert.That(result.TransactionType == TransactionType.Sell).IsTrue();
		await Assert.That(result.Units).IsEqualTo(4m);
		await Assert.That(result.Price).IsEqualTo(30m);
		await Assert.That(result.Amount).IsEqualTo(120m);
	}

	[Test]
	public async Task DeleteAsync_ThenGetByIdAsync_ReturnsEmpty()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new TransactionRepository(db, TimeProvider.System);
		var userId = await AddUserAsync(db);
		var asset = await AddAssetAsync(db);
		var assetItem = await AddAssetItemAsync(db, userId, asset.Id);

		var addedTransaction = await repository.AddAsync(
			userId,
			assetItem.Id,
			new DateOnly(2024, 8, 20),
			"To Delete",
			TransactionType.Deposit,
			1m,
			50m,
			50m,
			CancellationToken.None);

		await repository.DeleteAsync(userId, assetItem.Id, addedTransaction.Id, CancellationToken.None);

		var result = await repository.GetByIdAsync(userId, assetItem.Id, addedTransaction.Id, CancellationToken.None);

		await Assert.That(result.Id == TransactionId.Empty).IsTrue();
	}

	private static async Task<UserId> AddUserAsync(DbConnectionFactory db)
	{
		var repository = new UserRepository(db, TimeProvider.System);
		var userId = new UserId(Guid.NewGuid());
		await repository.AddUserAsync(
			userId,
			"linus@example.com",
			"Linus",
			"Torvalds",
			"Linus Torvalds",
			CancellationToken.None);
		return userId;
	}

	private static Task<Asset> AddAssetAsync(DbConnectionFactory db)
	{
		var repository = new AssetRepository(db, TimeProvider.System);
		return repository.AddAsync(
			"Tech Fund",
			AssetClass.Equity,
			AssetType.MutualFund,
			Currency.USD,
			"txn-asset-123",
			CancellationToken.None);
	}

	private static Task<AssetItem> AddAssetItemAsync(DbConnectionFactory db, UserId userId, AssetId assetId)
	{
		var repository = new AssetItemRepository(db, TimeProvider.System);
		return repository.AddAsync(userId, assetId, "Primary Portfolio", CancellationToken.None);
	}
}
