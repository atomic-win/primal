using System.Globalization;
using Dapper;
using Primal.Application.Users;
using Primal.Domain.Money;
using Primal.Domain.Users;
using Primal.Infrastructure.Persistence;

namespace Primal.Infrastructure.Users;

internal sealed class UserRepository : IUserRepository
{
	private readonly DbConnectionFactory connectionFactory;

	internal UserRepository(DbConnectionFactory connectionFactory)
	{
		this.connectionFactory = connectionFactory;
	}

	public async Task<User> GetUserAsync(
		UserId userId,
		CancellationToken cancellationToken)
	{
		using var connection = this.connectionFactory.CreateConnection();

		var row = await connection.QueryFirstOrDefaultAsync<UserTableEntity>(
			"SELECT * FROM users WHERE Id = @Id",
			new { Id = userId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant() });

		if (row is null)
		{
			return User.Empty;
		}

		return MapToUser(row);
	}

	public async Task<User> AddUserAsync(
		UserId userId,
		string email,
		string firstName,
		string lastName,
		string fullName,
		CancellationToken cancellationToken)
	{
		var now = DateTimeOffset.UtcNow.ToString("O");

		using var connection = this.connectionFactory.CreateConnection();

		await connection.ExecuteAsync(
			"""
			INSERT INTO users (Id, Email, FirstName, LastName, FullName, PreferredCurrency, PreferredLocale, CreatedAt, UpdatedAt)
			VALUES (@Id, @Email, @FirstName, @LastName, @FullName, @PreferredCurrency, @PreferredLocale, @CreatedAt, @UpdatedAt)
			""",
			new
			{
				Id = userId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				Email = email,
				FirstName = firstName,
				LastName = lastName,
				FullName = fullName,
				PreferredCurrency = Currency.USD.ToString(),
				PreferredLocale = Locale.EN_US.ToString(),
				CreatedAt = now,
				UpdatedAt = now,
			});

		return new User(
			userId,
			email,
			firstName,
			lastName,
			fullName,
			Currency.USD,
			Locale.EN_US);
	}

	public async Task UpdateUserProfileAsync(
		UserId userId,
		Currency preferredCurrency,
		Locale preferredLocale,
		CancellationToken cancellationToken)
	{
		var now = DateTimeOffset.UtcNow.ToString("O");

		using var connection = this.connectionFactory.CreateConnection();

		await connection.ExecuteAsync(
			"""
			UPDATE users SET PreferredCurrency = @PreferredCurrency, PreferredLocale = @PreferredLocale, UpdatedAt = @UpdatedAt
			WHERE Id = @Id
			""",
			new
			{
				Id = userId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				PreferredCurrency = preferredCurrency.ToString(),
				PreferredLocale = preferredLocale.ToString(),
				UpdatedAt = now,
			});
	}

	private static User MapToUser(UserTableEntity entity)
	{
		return new User(
			new UserId(Guid.Parse(entity.Id)),
			entity.Email,
			entity.FirstName,
			entity.LastName,
			entity.FullName,
			Enum.Parse<Currency>(entity.PreferredCurrency),
			Enum.Parse<Locale>(entity.PreferredLocale));
	}
}
