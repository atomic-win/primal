using System.Net.Mail;
using Primal.Domain.Common.Models;

namespace Primal.Domain.Users;

public sealed class IdentityProviderUser : Entity<IdentityProviderUserId>
{
	public IdentityProviderUser(
		IdentityProviderUserId id,
		IdentityProvider identityProvider,
		string email,
		string firstName,
		string lastName,
		string fullName,
		Uri profilePictureUrl)
		: base(id)
	{
		this.IdentityProvider = identityProvider;
		this.Email = new MailAddress(email);
		this.FirstName = firstName;
		this.LastName = lastName;
		this.FullName = fullName;
		this.ProfilePictureUrl = profilePictureUrl;
	}

	public IdentityProvider IdentityProvider { get; init; }

	public MailAddress Email { get; init; }

	public string FirstName { get; init; }

	public string LastName { get; init; }

	public string FullName { get; init; }

	public Uri ProfilePictureUrl { get; init; }
}
