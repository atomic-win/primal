using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;
using Primal.Infrastructure.Investments;
using Primal.Infrastructure.Users;

namespace Primal.Infrastructure.IntegrationTests.Investments.Repositories;

public sealed class AssetItemRepositoryTests
{
	[Test]
	public async Task Add_ThenGetAll_ReturnsItems()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var userRepo = new UserRepository(db, TimeProvider.System);
		var assetRepo = new AssetRepository(db, TimeProvider.System);
		var repository = new AssetItemRepository(db, TimeProvider.System);

		var user = await userRepo.AddUserAsync("test@example.com", "Test", "User", "Test User", CancellationToken.None);
		var asset = await assetRepo.AddAsync("Test", AssetClass.Equity, AssetType.MutualFund, Currency.INR, "mf-123", CancellationToken.None);

		await repository.AddAsync(user.Id, asset.Id, "My Fund", CancellationToken.None);

		var result = await repository.GetAllAsync(user.Id, CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetAll_Empty_ReturnsEmptyCollection()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var userRepo = new UserRepository(db, TimeProvider.System);
		var repository = new AssetItemRepository(db, TimeProvider.System);

		var user = await userRepo.AddUserAsync("test@example.com", "Test", "User", "Test User", CancellationToken.None);

		var result = await repository.GetAllAsync(user.Id, CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task Add_ThenGetById_ReturnsAssetItem()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var userRepo = new UserRepository(db, TimeProvider.System);
		var assetRepo = new AssetRepository(db, TimeProvider.System);
		var repository = new AssetItemRepository(db, TimeProvider.System);

		var user = await userRepo.AddUserAsync("test@example.com", "Test", "User", "Test User", CancellationToken.None);
		var asset = await assetRepo.AddAsync("Test", AssetClass.Equity, AssetType.MutualFund, Currency.INR, "mf-123", CancellationToken.None);

		var item = await repository.AddAsync(user.Id, asset.Id, "My Fund", CancellationToken.None);

		var result = await repository.GetByIdAsync(user.Id, item.Id, CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetById_NonExistent_ReturnsEmpty()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var userRepo = new UserRepository(db, TimeProvider.System);
		var repository = new AssetItemRepository(db, TimeProvider.System);

		var user = await userRepo.AddUserAsync("test@example.com", "Test", "User", "Test User", CancellationToken.None);

		var result = await repository.GetByIdAsync(user.Id, new AssetItemId(Guid.NewGuid()), CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task Delete_RemovesItem()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var userRepo = new UserRepository(db, TimeProvider.System);
		var assetRepo = new AssetRepository(db, TimeProvider.System);
		var repository = new AssetItemRepository(db, TimeProvider.System);

		var user = await userRepo.AddUserAsync("test@example.com", "Test", "User", "Test User", CancellationToken.None);
		var asset = await assetRepo.AddAsync("Test", AssetClass.Equity, AssetType.MutualFund, Currency.INR, "mf-123", CancellationToken.None);

		var item = await repository.AddAsync(user.Id, asset.Id, "My Fund", CancellationToken.None);

		await repository.DeleteAsync(user.Id, item.Id, CancellationToken.None);

		var result = await repository.GetByIdAsync(user.Id, item.Id, CancellationToken.None);

		await Verifier.Verify(result);
	}
}
