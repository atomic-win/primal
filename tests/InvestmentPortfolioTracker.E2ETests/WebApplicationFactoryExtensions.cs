using System.Globalization;
using Dapper;
using InvestmentPortfolioTracker.Domain.Users;
using Microsoft.Data.Sqlite;

namespace InvestmentPortfolioTracker.E2ETests;

internal static class WebApplicationFactoryExtensions
{
	internal static async Task<UserId> CreateUserAsync(this InvestmentPortfolioTrackerE2EFactory factory)
	{
		var userId = new UserId(Guid.NewGuid());
		var now = DateTimeOffset.UtcNow.ToString("O");

		// Inserts a test user directly into the database.
		// User creation requires Google OAuth which cannot be mocked at HTTP level.
		// Direct DB insert is the only option since there's no public user-creation API.
		using var connection = new SqliteConnection($"Data Source={factory.DbPath}");
		await connection.OpenAsync();

		await connection.ExecuteAsync(
			"""
			INSERT INTO users (Id, Email, FirstName, LastName, FullName, PreferredCurrency, PreferredLocale, CreatedAt, UpdatedAt)
			VALUES (@Id, @Email, @FirstName, @LastName, @FullName, @PreferredCurrency, @PreferredLocale, @CreatedAt, @UpdatedAt)
			""",
			new
			{
				Id = userId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				Email = "test@example.com",
				FirstName = "Test",
				LastName = "User",
				FullName = "Test User",
				PreferredCurrency = "USD",
				PreferredLocale = "EN_US",
				CreatedAt = now,
				UpdatedAt = now,
			});

		return userId;
	}
}
