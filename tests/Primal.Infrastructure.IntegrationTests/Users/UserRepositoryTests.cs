using Primal.Domain.Money;
using Primal.Domain.Users;
using Primal.Infrastructure.Users;

namespace Primal.Infrastructure.IntegrationTests.Users;

public sealed class UserRepositoryTests
{
	[Test]
	public async Task GetUserAsync_NonExistentUser_ReturnsEmptyUser()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new UserRepository(db);

		var result = await repository.GetUserAsync(new UserId(Guid.NewGuid()), CancellationToken.None);

		await Assert.That(result.Id == UserId.Empty).IsTrue();
		await Assert.That(string.Equals(result.Email, string.Empty, StringComparison.Ordinal)).IsTrue();
		await Assert.That(result.PreferredCurrency == Currency.Unknown).IsTrue();
		await Assert.That(result.PreferredLocale == Locale.Unknown).IsTrue();
	}

	[Test]
	public async Task AddUserAsync_ThenGetUserAsync_ReturnsCorrectUser()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new UserRepository(db);
		var userId = new UserId(Guid.NewGuid());

		await repository.AddUserAsync(
			userId,
			"ada@example.com",
			"Ada",
			"Lovelace",
			"Ada Lovelace",
			CancellationToken.None);

		var result = await repository.GetUserAsync(userId, CancellationToken.None);

		await Assert.That(result.Id == userId).IsTrue();
		await Assert.That(string.Equals(result.Email, "ada@example.com", StringComparison.Ordinal)).IsTrue();
		await Assert.That(string.Equals(result.FirstName, "Ada", StringComparison.Ordinal)).IsTrue();
		await Assert.That(string.Equals(result.LastName, "Lovelace", StringComparison.Ordinal)).IsTrue();
		await Assert.That(string.Equals(result.FullName, "Ada Lovelace", StringComparison.Ordinal)).IsTrue();
		await Assert.That(result.PreferredCurrency == Currency.USD).IsTrue();
		await Assert.That(result.PreferredLocale == Locale.EN_US).IsTrue();
	}

	[Test]
	public async Task UpdateUserProfileAsync_UpdatesPreferences()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new UserRepository(db);
		var userId = new UserId(Guid.NewGuid());

		await repository.AddUserAsync(
			userId,
			"ada@example.com",
			"Ada",
			"Lovelace",
			"Ada Lovelace",
			CancellationToken.None);

		await repository.UpdateUserProfileAsync(userId, Currency.INR, Locale.EN_IN, CancellationToken.None);

		var result = await repository.GetUserAsync(userId, CancellationToken.None);

		await Assert.That(result.Id == userId).IsTrue();
		await Assert.That(result.PreferredCurrency == Currency.INR).IsTrue();
		await Assert.That(result.PreferredLocale == Locale.EN_IN).IsTrue();
		await Assert.That(string.Equals(result.Email, "ada@example.com", StringComparison.Ordinal)).IsTrue();
	}
}
