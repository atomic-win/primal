using Primal.Domain.Users;

namespace Primal.Domain.UnitTests.Users;

public sealed class IdentityProviderUserTests
{
	[Test]
	public async Task Constructor_SetsAllProperties()
	{
		var id = new IdentityProviderUserId("google-123");
		var profileUrl = new Uri("https://example.com/photo.jpg");

		var user = new IdentityProviderUser(
			id,
			IdentityProvider.Google,
			"ada@example.com",
			"Ada",
			"Lovelace",
			"Ada Lovelace",
			profileUrl);

		await Assert.That(user.Id == id).IsTrue();
		await Assert.That(user.IdentityProvider == IdentityProvider.Google).IsTrue();
		await Assert.That(string.Equals(user.Email, "ada@example.com", StringComparison.Ordinal)).IsTrue();
		await Assert.That(string.Equals(user.FirstName, "Ada", StringComparison.Ordinal)).IsTrue();
		await Assert.That(string.Equals(user.LastName, "Lovelace", StringComparison.Ordinal)).IsTrue();
		await Assert.That(string.Equals(user.FullName, "Ada Lovelace", StringComparison.Ordinal)).IsTrue();
		await Assert.That(user.ProfilePictureUrl == profileUrl).IsTrue();
	}
}
