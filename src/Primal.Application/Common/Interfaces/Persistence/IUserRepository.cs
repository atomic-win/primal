using System.Net.Mail;
using ErrorOr;
using Primal.Domain.Users;

namespace Primal.Application.Common.Interfaces.Persistence;

public interface IUserRepository
{
	Task<ErrorOr<User>> GetUserAsync(
		UserId userId,
		CancellationToken cancellationToken);

	Task<ErrorOr<User>> AddUserAsync(
		MailAddress email,
		string firstName,
		string lastName,
		string fullName,
		Uri profilePictureUrl,
		CancellationToken cancellationToken);
}
