using System.Globalization;
using Dapper;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Infrastructure.Persistence;

namespace Primal.Infrastructure.Investments;

internal sealed class AssetRepository : IAssetRepository
{
	private readonly DbConnectionFactory connectionFactory;

	internal AssetRepository(DbConnectionFactory connectionFactory)
	{
		this.connectionFactory = connectionFactory;
	}

	public async Task<Asset> GetByIdAsync(AssetId assetId, CancellationToken cancellationToken)
	{
		using var connection = this.connectionFactory.CreateConnection();

		var row = await connection.QueryFirstOrDefaultAsync<AssetTableEntity>(
			"SELECT * FROM assets WHERE Id = @Id",
			new { Id = assetId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant() });

		if (row is null)
		{
			return Asset.Empty;
		}

		return MapToAsset(row);
	}

	public async Task<Asset> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken)
	{
		using var connection = this.connectionFactory.CreateConnection();

		var row = await connection.QueryFirstOrDefaultAsync<AssetTableEntity>(
			"SELECT * FROM assets WHERE ExternalId = @ExternalId",
			new { ExternalId = externalId });

		if (row is null)
		{
			return Asset.Empty;
		}

		return MapToAsset(row);
	}

	public async Task<Asset> AddAsync(
		string name,
		AssetClass assetClass,
		AssetType assetType,
		Currency currency,
		string externalId,
		CancellationToken cancellationToken)
	{
		var id = Guid.CreateVersion7();
		var now = DateTimeOffset.UtcNow.ToString("O");

		using var connection = this.connectionFactory.CreateConnection();

		await connection.ExecuteAsync(
			"""
			INSERT INTO assets (Id, Name, AssetClass, AssetType, Currency, ExternalId, CreatedAt, UpdatedAt)
			VALUES (@Id, @Name, @AssetClass, @AssetType, @Currency, @ExternalId, @CreatedAt, @UpdatedAt)
			""",
			new
			{
				Id = id.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				Name = name,
				AssetClass = assetClass.ToString(),
				AssetType = assetType.ToString(),
				Currency = currency.ToString(),
				ExternalId = externalId,
				CreatedAt = now,
				UpdatedAt = now,
			});

		return new Asset(
			new AssetId(id),
			name,
			assetClass,
			assetType,
			currency,
			externalId);
	}

	private static Asset MapToAsset(AssetTableEntity entity)
	{
		return new Asset(
			new AssetId(Guid.Parse(entity.Id)),
			entity.Name,
			Enum.Parse<AssetClass>(entity.AssetClass),
			Enum.Parse<AssetType>(entity.AssetType),
			Enum.Parse<Currency>(entity.Currency),
			entity.ExternalId);
	}
}
