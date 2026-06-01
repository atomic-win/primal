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

		var user = await repository.AddUserAsync(
			"ada@example.com",
			"Ada",
			"Lovelace",
			"Ada Lovelace",
			CancellationToken.None);

		var result = await repository.GetUserAsync(user.Id, CancellationToken.None);

		await Verifier.Verify(result);
	}
}
