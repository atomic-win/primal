using System.Net.Mail;
using Primal.Domain.Common.Models;

namespace Primal.Domain.Users;

public sealed class User : Entity<UserId>
{
	public User(UserId id, MailAddress email)
		: base(id)
	{
		this.Email = email;
	}

	public MailAddress Email { get; init; }
}
