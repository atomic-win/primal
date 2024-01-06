using System.Net.Mail;
using Primal.Domain.Common.Models;

namespace Primal.Domain.Users;

public sealed class IdentityProviderUser : Entity<IdentityProviderUserId>
{
	public IdentityProviderUser(IdentityProviderUserId id, IdentityProvider identityProvider, string email)
		: base(id)
	{
		this.IdentityProvider = identityProvider;
		this.Email = new MailAddress(email);
	}

	public IdentityProvider IdentityProvider { get; init; }

	public MailAddress Email { get; init; }
}
