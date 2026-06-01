using Primal.Domain.Users;
using Primal.Infrastructure.Users;

namespace Primal.Infrastructure.IntegrationTests.Users;

public sealed class UserIdRepositoryTests
{
	[Test]
	public async Task GetUserId_NonExistentUser_ReturnsEmptyUserId()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new UserIdRepository(db, TimeProvider.System);

		var result = await repository.GetUserId(
			IdentityProvider.Google,
			new IdentityProviderUserId("non-existent"),
			CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task AddUserId_ThenGetUserId_ReturnsCorrectUserId()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var userRepository = new UserRepository(db, TimeProvider.System);
		var repository = new UserIdRepository(db, TimeProvider.System);
		var identityProviderUserId = new IdentityProviderUserId("google-123");

		var user = await userRepository.AddUserAsync(
			"test@example.com",
			"Test",
			"User",
			"Test User",
			CancellationToken.None);

		await repository.AddUserId(
			IdentityProvider.Google,
			identityProviderUserId,
			user.Id,
			CancellationToken.None);

		var result = await repository.GetUserId(
			IdentityProvider.Google,
			identityProviderUserId,
			CancellationToken.None);

		await Assert.That(result).IsEqualTo(user.Id);
	}
}
