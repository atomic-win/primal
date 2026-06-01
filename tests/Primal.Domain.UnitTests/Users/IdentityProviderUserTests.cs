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

		await Verifier.Verify(user);
	}
}
