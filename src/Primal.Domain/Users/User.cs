using System.Net.Mail;
using Primal.Domain.Common.Models;

namespace Primal.Domain.Users;

public sealed class User : Entity<UserId>
{
	public User(
		UserId id,
		MailAddress email,
		string fullName)
		: base(id)
	{
		this.Email = email;
		this.FullName = fullName;
	}

	public static User Empty { get; } = new User(
		UserId.Empty,
		null,
		string.Empty);

	public MailAddress Email { get; init; }

	public string FullName { get; init; }
}
