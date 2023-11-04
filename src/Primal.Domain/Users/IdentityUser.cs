using System.Net.Mail;
using Primal.Domain.Common.Models;

namespace Primal.Domain.Users;

public sealed class IdentityUser : Entity<IdentityUserId>
{
	public IdentityUser(IdentityUserId id, string email)
		: base(id)
	{
		this.Email = new MailAddress(email);
	}

	public MailAddress Email { get; init; }
}
