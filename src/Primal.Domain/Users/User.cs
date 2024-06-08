using System.Net.Mail;
using Primal.Domain.Common.Models;

namespace Primal.Domain.Users;

public sealed class User : Entity<UserId>
{
	public User(
		UserId id,
		MailAddress email,
		string firstName,
		string lastName,
		string fullName,
		Uri profilePictureUrl)
		: base(id)
	{
		this.Email = email;
		this.FirstName = firstName;
		this.LastName = lastName;
		this.FullName = fullName;
		this.ProfilePictureUrl = profilePictureUrl;
	}

	public MailAddress Email { get; init; }

	public string FirstName { get; init; }

	public string LastName { get; init; }

	public string FullName { get; init; }

	public Uri ProfilePictureUrl { get; init; }
}
