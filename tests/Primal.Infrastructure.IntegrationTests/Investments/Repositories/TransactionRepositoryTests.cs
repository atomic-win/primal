using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;
using Primal.Infrastructure.Investments;
using Primal.Infrastructure.Users;

namespace Primal.Infrastructure.IntegrationTests.Investments.Repositories;

public sealed class TransactionRepositoryTests
{
	[Test]
	public async Task Add_ThenGetByAssetItemId_ReturnsTransactions()
	{
		var (repository, userId, assetItemId) = await CreateTestContext();

		await repository.AddAsync(userId, assetItemId, new DateOnly(2026, 1, 15), "Buy", TransactionType.Buy, 10, 100, 0, CancellationToken.None);

		var result = await repository.GetByAssetItemIdAsync(userId, assetItemId, CancellationToken.None);

		await Assert.That(result.Count()).IsEqualTo(1);
	}

	[Test]
	public async Task GetByAssetItemId_Empty_ReturnsEmptyCollection()
	{
		var (repository, userId, assetItemId) = await CreateTestContext();

		var result = await repository.GetByAssetItemIdAsync(userId, assetItemId, CancellationToken.None);

		await Assert.That(result.Count()).IsEqualTo(0);
	}

	[Test]
	public async Task Add_ThenGetById_ReturnsTransaction()
	{
		var (repository, userId, assetItemId) = await CreateTestContext();

		var transaction = await repository.AddAsync(userId, assetItemId, new DateOnly(2026, 1, 15), "Buy Units", TransactionType.Buy, 10, 150.25m, 0, CancellationToken.None);

		var result = await repository.GetByIdAsync(userId, assetItemId, transaction.Id, CancellationToken.None);

		await Assert.That(result.Id).IsEqualTo(transaction.Id);
		await Assert.That(result.Name).IsEqualTo("Buy Units");
	}

	[Test]
	public async Task GetById_NonExistent_ReturnsEmpty()
	{
		var (repository, userId, assetItemId) = await CreateTestContext();

		var result = await repository.GetByIdAsync(userId, assetItemId, new TransactionId(Guid.NewGuid()), CancellationToken.None);

		await Assert.That(result.Id).IsEqualTo(TransactionId.Empty);
	}

	[Test]
	public async Task Update_ModifiesTransaction()
	{
		var (repository, userId, assetItemId) = await CreateTestContext();

		var transaction = await repository.AddAsync(userId, assetItemId, new DateOnly(2026, 1, 15), "Buy Units", TransactionType.Buy, 10, 100, 0, CancellationToken.None);

		var updated = new Transaction(transaction.Id, new DateOnly(2026, 1, 15), "Updated Name", TransactionType.Buy, assetItemId, 20, 200, 0);
		await repository.UpdateAsync(userId, updated, CancellationToken.None);

		var result = await repository.GetByIdAsync(userId, assetItemId, transaction.Id, CancellationToken.None);

		await Assert.That(result.Name).IsEqualTo("Updated Name");
		await Assert.That(result.Units).IsEqualTo(20m);
		await Assert.That(result.Price).IsEqualTo(200m);
	}

	[Test]
	public async Task Delete_RemovesTransaction()
	{
		var (repository, userId, assetItemId) = await CreateTestContext();

		var transaction = await repository.AddAsync(userId, assetItemId, new DateOnly(2026, 1, 15), "Buy", TransactionType.Buy, 10, 100, 0, CancellationToken.None);

		await repository.DeleteAsync(userId, assetItemId, transaction.Id, CancellationToken.None);

		var result = await repository.GetByIdAsync(userId, assetItemId, transaction.Id, CancellationToken.None);

		await Assert.That(result.Id).IsEqualTo(TransactionId.Empty);
	}

	private static async Task<(TransactionRepository Repository, UserId UserId, AssetItemId AssetItemId)> CreateTestContext()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var userRepo = new UserRepository(db, TimeProvider.System);
		var assetRepo = new AssetRepository(db, TimeProvider.System);
		var assetItemRepo = new AssetItemRepository(db, TimeProvider.System);
		var transactionRepo = new TransactionRepository(db, TimeProvider.System);

		var user = await userRepo.AddUserAsync("test@example.com", "Test", "User", "Test User", CancellationToken.None);
		var asset = await assetRepo.AddAsync("Test", AssetClass.Equity, AssetType.MutualFund, Currency.INR, "mf-123", CancellationToken.None);
		var assetItem = await assetItemRepo.AddAsync(user.Id, asset.Id, "My Fund", CancellationToken.None);

		return (transactionRepo, user.Id, assetItem.Id);
	}
}
