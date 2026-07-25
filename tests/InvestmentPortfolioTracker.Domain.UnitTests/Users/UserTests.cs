using InvestmentPortfolioTracker.Domain.Money;
using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Domain.UnitTests.Users;

public sealed class UserTests
{
	[Test]
	public async Task Empty_ReturnsExpectedDefaultValues()
	{
		var user = User.Empty;

		await Verifier.Verify(user);
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

		await Verifier.Verify(user);
	}
}
