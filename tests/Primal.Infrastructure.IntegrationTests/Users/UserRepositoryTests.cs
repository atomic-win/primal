using Primal.Domain.Users;
using Primal.Infrastructure.Users;

namespace Primal.Infrastructure.IntegrationTests.Users;

public sealed class UserRepositoryTests
{
	[Test]
	public async Task AddUserAsync_ThenGetUserAsync_ReturnsCorrectUser()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new UserRepository(db, TimeProvider.System);
		var userId = new UserId(Guid.NewGuid());

		await repository.AddUserAsync(
			userId,
			"ada@example.com",
			"Ada",
			"Lovelace",
			"Ada Lovelace",
			CancellationToken.None);

		var result = await repository.GetUserAsync(userId, CancellationToken.None);

		await Verifier.Verify(result);
	}
}
