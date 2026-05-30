using Primal.Domain.Users;
using Primal.Infrastructure.Users;

namespace Primal.Infrastructure.IntegrationTests.Users;

public sealed class UserIdRepositoryTests
{
	[Test]
	public async Task GetUserId_NonExistentUser_ReturnsEmptyUserId()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new UserIdRepository(db);

		var result = await repository.GetUserId(
			IdentityProvider.Google,
			new IdentityProviderUserId("non-existent"),
			CancellationToken.None);

		await Assert.That(result == UserId.Empty).IsTrue();
	}

	[Test]
	public async Task AddUserId_ThenGetUserId_ReturnsCorrectUserId()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new UserIdRepository(db);
		var identityProviderUserId = new IdentityProviderUserId("google-123");

		var addedUserId = await repository.AddUserId(
			IdentityProvider.Google,
			identityProviderUserId,
			CancellationToken.None);

		var result = await repository.GetUserId(
			IdentityProvider.Google,
			identityProviderUserId,
			CancellationToken.None);

		await Assert.That(result == addedUserId).IsTrue();
	}
}
