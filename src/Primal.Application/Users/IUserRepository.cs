using System.Net.Mail;
using Primal.Domain.Users;

namespace Primal.Application.Users;

public interface IUserRepository
{
	Task<User> GetUserAsync(
		UserId userId,
		CancellationToken cancellationToken);

	Task<User> AddUserAsync(
		UserId userId,
		MailAddress email,
		string firstName,
		string lastName,
		string fullName,
		CancellationToken cancellationToken);
}
