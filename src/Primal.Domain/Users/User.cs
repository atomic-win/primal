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
		string fullName)
		: base(id)
	{
		this.Email = email;
		this.FirstName = firstName;
		this.LastName = lastName;
		this.FullName = fullName;
	}

	public static User Empty { get; } = new User(
		UserId.Empty,
		new MailAddress("empty@empty.com"),
		string.Empty,
		string.Empty,
		string.Empty);

	public MailAddress Email { get; init; }

	public string FirstName { get; init; }

	public string LastName { get; init; }

	public string FullName { get; init; }
}
