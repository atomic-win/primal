using System.Net.Mail;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Application.Users;

public interface IUserRepository
{
	Task<User> GetUserAsync(
		UserId userId,
		CancellationToken cancellationToken);

	Task<User> AddUserAsync(
		UserId userId,
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
