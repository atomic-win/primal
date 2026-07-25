using InvestmentPortfolioTracker.Domain.Money;
using InvestmentPortfolioTracker.Domain.Users;
using InvestmentPortfolioTracker.Infrastructure.Users;

namespace InvestmentPortfolioTracker.Infrastructure.IntegrationTests.Users;

public sealed class UserRepositoryTests
{
	[Test]
	public async Task AddUserAsync_ThenGetUserAsync_ReturnsCorrectUser()
	{
		var db = TestDbFactory.CreateTestDatabase();
		var repository = new UserRepository(db, TimeProvider.System);

		var user = await repository.AddUserAsync(
			"ada@example.com",
			"Ada",
			"Lovelace",
			"Ada Lovelace",
			CancellationToken.None);

		var result = await repository.GetUserAsync(user.Id, CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetUserAsync_NonExistent_ReturnsEmpty()
	{
		var db = TestDbFactory.CreateTestDatabase();
		var repository = new UserRepository(db, TimeProvider.System);

		var result = await repository.GetUserAsync(new UserId(Guid.NewGuid()), CancellationToken.None);

		await Assert.That(result).IsEqualTo(User.Empty);
	}

	[Test]
	public async Task UpdateUserProfileAsync_UpdatesFields()
	{
		var db = TestDbFactory.CreateTestDatabase();
		var repository = new UserRepository(db, TimeProvider.System);

		var user = await repository.AddUserAsync(
			"ada@example.com",
			"Ada",
			"Lovelace",
			"Ada Lovelace",
			CancellationToken.None);

		await repository.UpdateUserProfileAsync(
			user.Id,
			Currency.INR,
			Locale.EN_IN,
			CancellationToken.None);

		var result = await repository.GetUserAsync(user.Id, CancellationToken.None);

		await Verifier.Verify(result);
	}
}
