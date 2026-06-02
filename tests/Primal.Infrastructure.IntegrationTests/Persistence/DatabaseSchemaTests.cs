using Dapper;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Infrastructure.Investments;
using Primal.Infrastructure.Persistence;
using Primal.Infrastructure.Users;

namespace Primal.Infrastructure.IntegrationTests.Persistence;

public sealed class DatabaseSchemaTests
{
	[Test]
	public async Task UserDelete_CascadesToUserIds()
	{
		var db = CreateFkEnabledDb();
		var userRepo = new UserRepository(db, TimeProvider.System);
		var userIdRepo = new UserIdRepository(db, TimeProvider.System);

		var user = await userRepo.AddUserAsync("test@example.com", "Test", "User", "Test User", CancellationToken.None);
		await userIdRepo.AddUserId(Domain.Users.IdentityProvider.Google, new Domain.Users.IdentityProviderUserId("g-123"), user.Id, CancellationToken.None);

		using var conn = db.CreateConnection();
		await conn.ExecuteAsync("DELETE FROM users WHERE Id = @Id", new { Id = user.Id.Value.ToString("D").ToUpperInvariant() });

		var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM user_ids");
		await Assert.That(count).IsEqualTo(0);
	}

	[Test]
	public async Task UserDelete_CascadesToAssetItems()
	{
		var db = CreateFkEnabledDb();
		var userRepo = new UserRepository(db, TimeProvider.System);
		var assetRepo = new AssetRepository(db, TimeProvider.System);
		var assetItemRepo = new AssetItemRepository(db, TimeProvider.System);

		var user = await userRepo.AddUserAsync("test@example.com", "Test", "User", "Test User", CancellationToken.None);
		var asset = await assetRepo.AddAsync("Test", AssetClass.Equity, AssetType.MutualFund, Currency.INR, "mf-123", CancellationToken.None);
		await assetItemRepo.AddAsync(user.Id, asset.Id, "My Fund", CancellationToken.None);

		using var conn = db.CreateConnection();
		await conn.ExecuteAsync("DELETE FROM users WHERE Id = @Id", new { Id = user.Id.Value.ToString("D").ToUpperInvariant() });

		var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM asset_items");
		await Assert.That(count).IsEqualTo(0);
	}

	[Test]
	public async Task UserDelete_CascadesToTransactions()
	{
		var db = CreateFkEnabledDb();
		var userRepo = new UserRepository(db, TimeProvider.System);
		var assetRepo = new AssetRepository(db, TimeProvider.System);
		var assetItemRepo = new AssetItemRepository(db, TimeProvider.System);
		var txnRepo = new TransactionRepository(db, TimeProvider.System);

		var user = await userRepo.AddUserAsync("test@example.com", "Test", "User", "Test User", CancellationToken.None);
		var asset = await assetRepo.AddAsync("Test", AssetClass.Equity, AssetType.MutualFund, Currency.INR, "mf-123", CancellationToken.None);
		var item = await assetItemRepo.AddAsync(user.Id, asset.Id, "My Fund", CancellationToken.None);
		await txnRepo.AddAsync(user.Id, item.Id, new DateOnly(2026, 1, 15), "Buy", TransactionType.Buy, 10, 100, 0, CancellationToken.None);

		using var conn = db.CreateConnection();
		await conn.ExecuteAsync("DELETE FROM users WHERE Id = @Id", new { Id = user.Id.Value.ToString("D").ToUpperInvariant() });

		var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM transactions");
		await Assert.That(count).IsEqualTo(0);
	}

	[Test]
	public async Task AssetItemDelete_CascadesToTransactions()
	{
		var db = CreateFkEnabledDb();
		var userRepo = new UserRepository(db, TimeProvider.System);
		var assetRepo = new AssetRepository(db, TimeProvider.System);
		var assetItemRepo = new AssetItemRepository(db, TimeProvider.System);
		var txnRepo = new TransactionRepository(db, TimeProvider.System);

		var user = await userRepo.AddUserAsync("test@example.com", "Test", "User", "Test User", CancellationToken.None);
		var asset = await assetRepo.AddAsync("Test", AssetClass.Equity, AssetType.MutualFund, Currency.INR, "mf-123", CancellationToken.None);
		var item = await assetItemRepo.AddAsync(user.Id, asset.Id, "My Fund", CancellationToken.None);
		await txnRepo.AddAsync(user.Id, item.Id, new DateOnly(2026, 1, 15), "Buy", TransactionType.Buy, 10, 100, 0, CancellationToken.None);

		using var conn = db.CreateConnection();
		await conn.ExecuteAsync("DELETE FROM asset_items WHERE Id = @Id", new { Id = item.Id.Value.ToString("D").ToUpperInvariant() });

		var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM transactions");
		await Assert.That(count).IsEqualTo(0);
	}

	[Test]
	public async Task DuplicateEmail_ThrowsUniqueConstraint()
	{
		var db = CreateFkEnabledDb();
		var userRepo = new UserRepository(db, TimeProvider.System);

		await userRepo.AddUserAsync("dup@example.com", "First", "User", "First User", CancellationToken.None);

		await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(async () =>
			await userRepo.AddUserAsync("dup@example.com", "Second", "User", "Second User", CancellationToken.None));
	}

	[Test]
	public async Task DuplicateExternalId_ThrowsUniqueConstraint()
	{
		var db = CreateFkEnabledDb();
		var assetRepo = new AssetRepository(db, TimeProvider.System);

		await assetRepo.AddAsync("First", AssetClass.Equity, AssetType.MutualFund, Currency.INR, "mf-dup", CancellationToken.None);

		await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(async () =>
			await assetRepo.AddAsync("Second", AssetClass.Debt, AssetType.MutualFund, Currency.INR, "mf-dup", CancellationToken.None));
	}

	private static DbConnectionFactory CreateFkEnabledDb()
	{
		var connectionFactory = new DbConnectionFactory($"Data Source=file:{Guid.NewGuid()}?mode=memory&cache=shared;Foreign Keys=True");
		var keepAliveConnection = connectionFactory.CreateConnection();
		keepAliveConnection.Open();
		DatabaseInitializer.Initialize(connectionFactory);
		return connectionFactory;
	}
}
