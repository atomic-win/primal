using Primal.Domain.Money;
using Primal.Domain.Users;
using Primal.Infrastructure.Users;

namespace Primal.Infrastructure.IntegrationTests.Users;

public sealed class CachedUserRepositoryTests
{
	[Test]
	public async Task GetUserAsync_SecondCall_ReturnsFromCache()
	{
		var cache = TestCacheFactory.CreateHybridCache();
		var db = TestDbFactory.CreateTestDatabase();
		var inner = new UserRepository(db, TimeProvider.System);
		var cached = new CachedUserRepository(cache, inner);

		var user = await inner.AddUserAsync("test@example.com", "Test", "User", "Test User", CancellationToken.None);

		var first = await cached.GetUserAsync(user.Id, CancellationToken.None);
		var second = await cached.GetUserAsync(user.Id, CancellationToken.None);

		await Verifier.Verify(new { first, second });
	}

	[Test]
	public async Task UpdateUserProfileAsync_InvalidatesCache()
	{
		var cache = TestCacheFactory.CreateHybridCache();
		var db = TestDbFactory.CreateTestDatabase();
		var inner = new UserRepository(db, TimeProvider.System);
		var cached = new CachedUserRepository(cache, inner);

		var user = await inner.AddUserAsync("test@example.com", "Test", "User", "Test User", CancellationToken.None);

		// Populate cache
		await cached.GetUserAsync(user.Id, CancellationToken.None);

		// Update via cached repo (invalidates cache)
		await cached.UpdateUserProfileAsync(user.Id, Currency.INR, Locale.EN_IN, CancellationToken.None);

		// Get should return updated data
		var result = await cached.GetUserAsync(user.Id, CancellationToken.None);

		await Verifier.Verify(result);
	}
}
