using Primal.Domain.Common.Models;
using Primal.Domain.Money;

namespace Primal.Domain.Users;

public sealed class User : Entity<UserId>
{
	public User(
		UserId id,
		string email,
		string firstName,
		string lastName,
		string fullName,
		Currency preferredCurrency,
		Locale preferredLocale)
		: base(id)
	{
		this.Email = email;
		this.FirstName = firstName;
		this.LastName = lastName;
		this.FullName = fullName;
		this.PreferredCurrency = preferredCurrency;
		this.PreferredLocale = preferredLocale;
	}

	public static User Empty { get; } = new User(
		UserId.Empty,
		string.Empty,
		string.Empty,
		string.Empty,
		string.Empty,
		Currency.Unknown,
		Locale.Unknown);

	public string Email { get; init; }

	public string FirstName { get; init; }

	public string LastName { get; init; }

	public string FullName { get; init; }

	public Currency PreferredCurrency { get; init; }

	public Locale PreferredLocale { get; init; }
}
