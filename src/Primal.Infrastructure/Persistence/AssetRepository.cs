using System.Collections.Immutable;
using ErrorOr;
using LiteDB;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;
using Primal.Domain.Users;
using SequentialGuid;

namespace Primal.Infrastructure.Persistence;

internal sealed class AssetRepository : IAssetRepository
{
	private readonly LiteDatabase liteDatabase;

	internal AssetRepository(LiteDatabase liteDatabase)
	{
		this.liteDatabase = liteDatabase;

		var collection = this.liteDatabase.GetCollection<AssetTableEntity>("Assets");
		collection.EnsureIndex(x => x.Id, unique: true);
		collection.EnsureIndex(x => x.UserId);
	}

	public async Task<ErrorOr<IEnumerable<Asset>>> GetAllAsync(UserId userId, CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var collection = this.liteDatabase.GetCollection<AssetTableEntity>("Assets");

		var assetTableEntities = collection.Find(x => x.UserId == userId.Value);

		return assetTableEntities.Select(this.MapToAsset).ToImmutableArray();
	}

	public async Task<ErrorOr<Asset>> GetByIdAsync(UserId userId, AssetId assetId, CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var collection = this.liteDatabase.GetCollection<AssetTableEntity>("Assets");

		var assetTableEntity = collection.FindOne(x => x.Id == assetId.Value && x.UserId == userId.Value);

		if (assetTableEntity == null)
		{
			return Error.NotFound(description: "Asset does not exist");
		}

		return this.MapToAsset(assetTableEntity);
	}

	public async Task<ErrorOr<Asset>> AddAsync(UserId userId, string name, InstrumentId instrumentId, CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var collection = this.liteDatabase.GetCollection<AssetTableEntity>("Assets");

		if (collection.FindOne(x => x.UserId == userId.Value && x.Name == name) != null)
		{
			return Error.Conflict(description: "Asset with the same name already exists");
		}

		var assetTableEntity = new AssetTableEntity
		{
			Id = SequentialGuidGenerator.Instance.NewGuid(),
			UserId = userId.Value,
			Name = name,
			InstrumentId = instrumentId.Value,
		};

		collection.Insert(assetTableEntity);

		return this.MapToAsset(assetTableEntity);
	}

	public async Task<ErrorOr<Success>> DeleteAsync(UserId userId, AssetId assetId, CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var collection = this.liteDatabase.GetCollection<AssetTableEntity>("Assets");

		var assetTableEntity = collection.FindOne(x => x.Id == assetId.Value && x.UserId == userId.Value);

		if (assetTableEntity == null)
		{
			return Error.NotFound(description: "Asset does not exist");
		}

		collection.Delete(assetId.Value);

		return Result.Success;
	}

	private Asset MapToAsset(AssetTableEntity assetTableEntity)
	{
		return new Asset(
			new AssetId(assetTableEntity.Id),
			assetTableEntity.Name,
			new InstrumentId(assetTableEntity.InstrumentId));
	}

	private sealed class AssetTableEntity
	{
		public Guid Id { get; set; }

		public Guid UserId { get; set; }

		public string Name { get; set; }

		public Guid InstrumentId { get; set; }
	}
}
