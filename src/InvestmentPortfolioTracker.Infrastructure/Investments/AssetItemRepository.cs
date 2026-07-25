using System.Globalization;
using Dapper;
using InvestmentPortfolioTracker.Core.Investments;
using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Users;
using InvestmentPortfolioTracker.Infrastructure.Persistence;

namespace InvestmentPortfolioTracker.Infrastructure.Investments;

internal sealed class AssetItemRepository : IAssetItemRepository
{
	private readonly DbConnectionFactory connectionFactory;
	private readonly TimeProvider timeProvider;

	internal AssetItemRepository(DbConnectionFactory connectionFactory, TimeProvider timeProvider)
	{
		this.connectionFactory = connectionFactory;
		this.timeProvider = timeProvider;
	}

	public async Task<IEnumerable<AssetItem>> GetAllAsync(
		UserId userId,
		CancellationToken cancellationToken)
	{
		using var connection = this.connectionFactory.CreateConnection();

		var rows = await connection.QueryAsync<AssetItemRow>(
			"SELECT Id, Name, UserId, AssetId FROM asset_items WHERE UserId = @UserId",
			new { UserId = userId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant() });

		return rows.Select(MapToAssetItem);
	}

	public async Task<AssetItem> GetByIdAsync(
		UserId userId,
		AssetItemId assetItemId,
		CancellationToken cancellationToken)
	{
		using var connection = this.connectionFactory.CreateConnection();

		var row = await connection.QueryFirstOrDefaultAsync<AssetItemRow>(
			"SELECT Id, Name, UserId, AssetId FROM asset_items WHERE UserId = @UserId AND Id = @Id",
			new { UserId = userId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(), Id = assetItemId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant() });

		if (row is null)
		{
			return AssetItem.Empty;
		}

		return MapToAssetItem(row);
	}

	public async Task<AssetItem> AddAsync(
		UserId userId,
		AssetId assetId,
		string name,
		CancellationToken cancellationToken)
	{
		var id = Guid.CreateVersion7();
		var now = this.timeProvider.GetUtcNow().ToString("O");

		using var connection = this.connectionFactory.CreateConnection();

		await connection.ExecuteAsync(
			"""
			INSERT INTO asset_items (Id, Name, UserId, AssetId, CreatedAt, UpdatedAt)
			VALUES (@Id, @Name, @UserId, @AssetId, @CreatedAt, @UpdatedAt)
			""",
			new
			{
				Id = id.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				Name = name,
				UserId = userId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				AssetId = assetId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				CreatedAt = now,
				UpdatedAt = now,
			});

		return new AssetItem(
			new AssetItemId(id),
			assetId,
			name);
	}

	public async Task UpdateAsync(
		UserId userId,
		AssetItem assetItem,
		CancellationToken cancellationToken)
	{
		var now = this.timeProvider.GetUtcNow().ToString("O");

		using var connection = this.connectionFactory.CreateConnection();

		await connection.ExecuteAsync(
			"UPDATE asset_items SET Name = @Name, UpdatedAt = @UpdatedAt WHERE UserId = @UserId AND Id = @Id",
			new
			{
				Name = assetItem.Name,
				UpdatedAt = now,
				UserId = userId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				Id = assetItem.Id.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
			});
	}

	public async Task DeleteAsync(
		UserId userId,
		AssetItemId assetItemId,
		CancellationToken cancellationToken)
	{
		using var connection = this.connectionFactory.CreateConnection();

		await connection.ExecuteAsync(
			"DELETE FROM asset_items WHERE UserId = @UserId AND Id = @Id",
			new { UserId = userId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(), Id = assetItemId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant() });
	}

	private static AssetItem MapToAssetItem(AssetItemRow row)
	{
		return new AssetItem(
			new AssetItemId(Guid.Parse(row.Id)),
			new AssetId(Guid.Parse(row.AssetId)),
			row.Name);
	}

	private sealed record AssetItemRow(
		string Id,
		string Name,
		string UserId,
		string AssetId);
}
