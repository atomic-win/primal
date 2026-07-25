using InvestmentPortfolioTracker.Domain.Money;
using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Core.Users;

public interface IUserRepository
{
	Task<User> GetUserAsync(
		UserId userId,
		CancellationToken cancellationToken);

	Task<User> AddUserAsync(
		string email,
		string firstName,
		string lastName,
		string fullName,
		CancellationToken cancellationToken);

	Task UpdateUserProfileAsync(
		UserId userId,
		Currency preferredCurrency,
		Locale preferredLocale,
		CancellationToken cancellationToken);
}
