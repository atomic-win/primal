using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Domain.UnitTests.Users;

public sealed class UserTests
{
	[Test]
	public async Task Empty_ReturnsExpectedDefaultValues()
	{
		var user = User.Empty;

		await Assert.That(user.Id == UserId.Empty).IsTrue();
		await Assert.That(string.Equals(user.Email, string.Empty, StringComparison.Ordinal)).IsTrue();
		await Assert.That(string.Equals(user.FirstName, string.Empty, StringComparison.Ordinal)).IsTrue();
		await Assert.That(string.Equals(user.LastName, string.Empty, StringComparison.Ordinal)).IsTrue();
		await Assert.That(string.Equals(user.FullName, string.Empty, StringComparison.Ordinal)).IsTrue();
		await Assert.That(user.PreferredCurrency == Currency.Unknown).IsTrue();
		await Assert.That(user.PreferredLocale == Locale.Unknown).IsTrue();
	}

	[Test]
	public async Task Constructor_SetsAllProperties()
	{
		var id = new UserId(Guid.NewGuid());
		var user = new User(
			id,
			"ada@example.com",
			"Ada",
			"Lovelace",
			"Ada Lovelace",
			Currency.USD,
			Locale.EN_US);

		await Assert.That(user.Id == id).IsTrue();
		await Assert.That(string.Equals(user.Email, "ada@example.com", StringComparison.Ordinal)).IsTrue();
		await Assert.That(string.Equals(user.FirstName, "Ada", StringComparison.Ordinal)).IsTrue();
		await Assert.That(string.Equals(user.LastName, "Lovelace", StringComparison.Ordinal)).IsTrue();
		await Assert.That(string.Equals(user.FullName, "Ada Lovelace", StringComparison.Ordinal)).IsTrue();
		await Assert.That(user.PreferredCurrency == Currency.USD).IsTrue();
		await Assert.That(user.PreferredLocale == Locale.EN_US).IsTrue();
	}
}
