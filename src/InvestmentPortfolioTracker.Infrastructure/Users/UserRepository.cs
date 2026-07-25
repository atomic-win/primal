using System.Globalization;
using Dapper;
using InvestmentPortfolioTracker.Core.Users;
using InvestmentPortfolioTracker.Domain.Money;
using InvestmentPortfolioTracker.Domain.Users;
using InvestmentPortfolioTracker.Infrastructure.Persistence;

namespace InvestmentPortfolioTracker.Infrastructure.Users;

internal sealed class UserRepository : IUserRepository
{
	private readonly DbConnectionFactory connectionFactory;
	private readonly TimeProvider timeProvider;

	internal UserRepository(DbConnectionFactory connectionFactory, TimeProvider timeProvider)
	{
		this.connectionFactory = connectionFactory;
		this.timeProvider = timeProvider;
	}

	public async Task<User> GetUserAsync(
		UserId userId,
		CancellationToken cancellationToken)
	{
		using var connection = this.connectionFactory.CreateConnection();

		var row = await connection.QueryFirstOrDefaultAsync<UserRow>(
			"SELECT Id, Email, FirstName, LastName, FullName, PreferredCurrency, PreferredLocale FROM users WHERE Id = @Id",
			new { Id = userId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant() });

		if (row is null)
		{
			return User.Empty;
		}

		return MapToUser(row);
	}

	public async Task<User> AddUserAsync(
		string email,
		string firstName,
		string lastName,
		string fullName,
		CancellationToken cancellationToken)
	{
		var userId = new UserId(Guid.CreateVersion7());
		var now = this.timeProvider.GetUtcNow().ToString("O");

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
		var now = this.timeProvider.GetUtcNow().ToString("O");

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

	private static User MapToUser(UserRow row)
	{
		return new User(
			new UserId(Guid.Parse(row.Id)),
			row.Email,
			row.FirstName,
			row.LastName,
			row.FullName,
			Enum.Parse<Currency>(row.PreferredCurrency),
			Enum.Parse<Locale>(row.PreferredLocale));
	}

	private sealed record UserRow(
		string Id,
		string Email,
		string FirstName,
		string LastName,
		string FullName,
		string PreferredCurrency,
		string PreferredLocale);
}
