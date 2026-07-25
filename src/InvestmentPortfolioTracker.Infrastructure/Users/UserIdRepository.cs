using System.Globalization;
using Dapper;
using InvestmentPortfolioTracker.Core.Users;
using InvestmentPortfolioTracker.Domain.Users;
using InvestmentPortfolioTracker.Infrastructure.Persistence;

namespace InvestmentPortfolioTracker.Infrastructure.Users;

internal sealed class UserIdRepository : IUserIdRepository
{
	private readonly DbConnectionFactory connectionFactory;
	private readonly TimeProvider timeProvider;

	internal UserIdRepository(DbConnectionFactory connectionFactory, TimeProvider timeProvider)
	{
		this.connectionFactory = connectionFactory;
		this.timeProvider = timeProvider;
	}

	public async Task<UserId> GetUserId(
		IdentityProvider identityProvider,
		IdentityProviderUserId identityProviderUserId,
		CancellationToken cancellationToken)
	{
		using var connection = this.connectionFactory.CreateConnection();

		var userId = await connection.QueryFirstOrDefaultAsync<string>(
			"SELECT UserId FROM user_ids WHERE IdentityProvider = @IdentityProvider AND Id = @Id",
			new { IdentityProvider = identityProvider.ToString(), Id = identityProviderUserId.Value });

		return userId is null ? UserId.Empty : new UserId(Guid.Parse(userId));
	}

	public async Task AddUserId(
		IdentityProvider identityProvider,
		IdentityProviderUserId identityProviderUserId,
		UserId userId,
		CancellationToken cancellationToken)
	{
		var now = this.timeProvider.GetUtcNow().ToString("O");

		using var connection = this.connectionFactory.CreateConnection();

		await connection.ExecuteAsync(
			"""
			INSERT INTO user_ids (Id, IdentityProvider, UserId, CreatedAt, UpdatedAt)
			VALUES (@Id, @IdentityProvider, @UserId, @CreatedAt, @UpdatedAt)
			""",
			new
			{
				Id = identityProviderUserId.Value,
				IdentityProvider = identityProvider.ToString(),
				UserId = userId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				CreatedAt = now,
				UpdatedAt = now,
			});
	}
}
