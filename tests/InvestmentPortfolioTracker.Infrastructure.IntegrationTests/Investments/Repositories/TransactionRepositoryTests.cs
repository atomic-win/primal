using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Money;
using InvestmentPortfolioTracker.Domain.Users;
using InvestmentPortfolioTracker.Infrastructure.Investments;
using InvestmentPortfolioTracker.Infrastructure.Users;

namespace InvestmentPortfolioTracker.Infrastructure.IntegrationTests.Investments.Repositories;

public sealed class TransactionRepositoryTests
{
	[Test]
	public async Task Add_ThenGetByAssetItemId_ReturnsTransactions()
	{
		var (repository, userId, assetItemId) = await CreateTestContext();

		await repository.AddAsync(userId, assetItemId, new DateOnly(2026, 1, 15), "Buy", TransactionType.Buy, 10, 100, 0, CancellationToken.None);

		var result = await repository.GetByAssetItemIdAsync(userId, assetItemId, CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetByAssetItemId_Empty_ReturnsEmptyCollection()
	{
		var (repository, userId, assetItemId) = await CreateTestContext();

		var result = await repository.GetByAssetItemIdAsync(userId, assetItemId, CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task Add_ThenGetById_ReturnsTransaction()
	{
		var (repository, userId, assetItemId) = await CreateTestContext();

		var transaction = await repository.AddAsync(userId, assetItemId, new DateOnly(2026, 1, 15), "Buy Units", TransactionType.Buy, 10, 150.25m, 0, CancellationToken.None);

		var result = await repository.GetByIdAsync(userId, assetItemId, transaction.Id, CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetById_NonExistent_ReturnsEmpty()
	{
		var (repository, userId, assetItemId) = await CreateTestContext();

		var result = await repository.GetByIdAsync(userId, assetItemId, new TransactionId(Guid.NewGuid()), CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task Update_ModifiesTransaction()
	{
		var (repository, userId, assetItemId) = await CreateTestContext();

		var transaction = await repository.AddAsync(userId, assetItemId, new DateOnly(2026, 1, 15), "Buy Units", TransactionType.Buy, 10, 100, 0, CancellationToken.None);

		var updated = new Transaction(transaction.Id, new DateOnly(2026, 1, 15), "Updated Name", TransactionType.Buy, assetItemId, 20, 200, 0);
		await repository.UpdateAsync(userId, assetItemId, updated, CancellationToken.None);

		var result = await repository.GetByIdAsync(userId, assetItemId, transaction.Id, CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task Delete_RemovesTransaction()
	{
		var (repository, userId, assetItemId) = await CreateTestContext();

		var transaction = await repository.AddAsync(userId, assetItemId, new DateOnly(2026, 1, 15), "Buy", TransactionType.Buy, 10, 100, 0, CancellationToken.None);

		await repository.DeleteAsync(userId, assetItemId, transaction.Id, CancellationToken.None);

		var result = await repository.GetByIdAsync(userId, assetItemId, transaction.Id, CancellationToken.None);

		await Verifier.Verify(result);
	}

	private static async Task<(TransactionRepository Repository, UserId UserId, AssetItemId AssetItemId)> CreateTestContext()
	{
		var db = TestDbFactory.CreateTestDatabase();
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
