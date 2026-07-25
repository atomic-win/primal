using System.Globalization;
using Dapper;
using InvestmentPortfolioTracker.Core.Investments;
using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Users;
using InvestmentPortfolioTracker.Infrastructure.Persistence;

namespace InvestmentPortfolioTracker.Infrastructure.Investments;

internal sealed class TransactionRepository : ITransactionRepository
{
	private readonly DbConnectionFactory connectionFactory;
	private readonly TimeProvider timeProvider;

	internal TransactionRepository(DbConnectionFactory connectionFactory, TimeProvider timeProvider)
	{
		this.connectionFactory = connectionFactory;
		this.timeProvider = timeProvider;
	}

	public async Task<IEnumerable<Transaction>> GetByAssetItemIdAsync(
		UserId userId,
		AssetItemId assetItemId,
		CancellationToken cancellationToken)
	{
		using var connection = this.connectionFactory.CreateConnection();

		var rows = await connection.QueryAsync<TransactionRow>(
			"SELECT Id, Date, Name, TransactionType, AssetItemId, UserId, Units, Price, Amount FROM transactions WHERE UserId = @UserId AND AssetItemId = @AssetItemId",
			new { UserId = userId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(), AssetItemId = assetItemId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant() });

		return rows.Select(MapToTransaction);
	}

	public async Task<Transaction> GetByIdAsync(
		UserId userId,
		AssetItemId assetItemId,
		TransactionId transactionId,
		CancellationToken cancellationToken)
	{
		using var connection = this.connectionFactory.CreateConnection();

		var row = await connection.QueryFirstOrDefaultAsync<TransactionRow>(
			"SELECT Id, Date, Name, TransactionType, AssetItemId, UserId, Units, Price, Amount FROM transactions WHERE UserId = @UserId AND AssetItemId = @AssetItemId AND Id = @Id",
			new
			{
				UserId = userId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				AssetItemId = assetItemId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				Id = transactionId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
			});

		if (row is null)
		{
			return Transaction.Empty;
		}

		return MapToTransaction(row);
	}

	public async Task<Transaction> AddAsync(
		UserId userId,
		AssetItemId assetItemId,
		DateOnly date,
		string name,
		TransactionType type,
		decimal units,
		decimal price,
		decimal amount,
		CancellationToken cancellationToken)
	{
		var id = Guid.CreateVersion7();
		var now = this.timeProvider.GetUtcNow().ToString("O");

		using var connection = this.connectionFactory.CreateConnection();

		await connection.ExecuteAsync(
			"""
			INSERT INTO transactions (Id, Date, Name, TransactionType, AssetItemId, UserId, Units, Price, Amount, CreatedAt, UpdatedAt)
			VALUES (@Id, @Date, @Name, @TransactionType, @AssetItemId, @UserId, @Units, @Price, @Amount, @CreatedAt, @UpdatedAt)
			""",
			new
			{
				Id = id.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				Date = date.ToString("O"),
				Name = name,
				TransactionType = type.ToString(),
				AssetItemId = assetItemId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				UserId = userId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				Units = units.ToString(CultureInfo.InvariantCulture),
				Price = price.ToString(CultureInfo.InvariantCulture),
				Amount = amount.ToString(CultureInfo.InvariantCulture),
				CreatedAt = now,
				UpdatedAt = now,
			});

		return new Transaction(
			new TransactionId(id),
			date,
			name,
			type,
			assetItemId,
			units,
			price,
			amount);
	}

	public async Task UpdateAsync(
		UserId userId,
		Transaction transaction,
		CancellationToken cancellationToken)
	{
		var now = this.timeProvider.GetUtcNow().ToString("O");

		using var connection = this.connectionFactory.CreateConnection();

		await connection.ExecuteAsync(
			"""
			UPDATE transactions
			SET Date = @Date, Name = @Name, TransactionType = @TransactionType,
				Units = @Units, Price = @Price, Amount = @Amount, UpdatedAt = @UpdatedAt
			WHERE UserId = @UserId AND Id = @Id
			""",
			new
			{
				Id = transaction.Id.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				UserId = userId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				Date = transaction.Date.ToString("O"),
				Name = transaction.Name,
				TransactionType = transaction.TransactionType.ToString(),
				Units = transaction.Units.ToString(CultureInfo.InvariantCulture),
				Price = transaction.Price.ToString(CultureInfo.InvariantCulture),
				Amount = transaction.Amount.ToString(CultureInfo.InvariantCulture),
				UpdatedAt = now,
			});
	}

	public async Task DeleteAsync(
		UserId userId,
		AssetItemId assetItemId,
		TransactionId transactionId,
		CancellationToken cancellationToken)
	{
		using var connection = this.connectionFactory.CreateConnection();

		await connection.ExecuteAsync(
			"DELETE FROM transactions WHERE UserId = @UserId AND AssetItemId = @AssetItemId AND Id = @Id",
			new
			{
				UserId = userId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				AssetItemId = assetItemId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				Id = transactionId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
			});
	}

	private static Transaction MapToTransaction(TransactionRow row)
	{
		return new Transaction(
			new TransactionId(Guid.Parse(row.Id)),
			DateOnly.Parse(row.Date, CultureInfo.InvariantCulture),
			row.Name,
			Enum.Parse<TransactionType>(row.TransactionType),
			new AssetItemId(Guid.Parse(row.AssetItemId)),
			decimal.Parse(row.Units, CultureInfo.InvariantCulture),
			decimal.Parse(row.Price, CultureInfo.InvariantCulture),
			decimal.Parse(row.Amount, CultureInfo.InvariantCulture));
	}

	private sealed record TransactionRow(
		string Id,
		string Date,
		string Name,
		string TransactionType,
		string AssetItemId,
		string UserId,
		string Units,
		string Price,
		string Amount);
}
