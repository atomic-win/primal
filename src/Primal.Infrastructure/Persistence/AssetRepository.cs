using Azure;
using Azure.Data.Tables;
using ErrorOr;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Persistence;

internal sealed class AssetRepository : IAssetRepository
{
	private readonly TableClient tableClient;

	internal AssetRepository(TableClient tableClient)
	{
		this.tableClient = tableClient;
	}

	public async Task<ErrorOr<IEnumerable<Asset>>> GetAllAsync(UserId userId, CancellationToken cancellationToken)
	{
		AsyncPageable<AssetTableEntity> entities = this.tableClient.QueryAsync<AssetTableEntity>(
			entity => entity.PartitionKey == userId.Value.ToString("N"),
			cancellationToken: cancellationToken);

		List<Asset> assets = new List<Asset>();

		await foreach (AssetTableEntity entity in entities.WithCancellation(cancellationToken))
		{
			assets.Add(new Asset(
				new AssetId(Guid.Parse(entity.RowKey)),
				entity.Name,
				new InstrumentId(Guid.Parse(entity.InstrumentId))));
		}

		return assets;
	}

	public async Task<ErrorOr<Asset>> GetByIdAsync(UserId userId, AssetId assetId, CancellationToken cancellationToken)
	{
		try
		{
			AssetTableEntity entity = await this.tableClient.GetEntityAsync<AssetTableEntity>(
				userId.Value.ToString("N"),
				assetId.Value.ToString("N"),
				cancellationToken: cancellationToken);

			return new Asset(
				new AssetId(Guid.Parse(entity.RowKey)),
				entity.Name,
				new InstrumentId(Guid.Parse(entity.InstrumentId)));
		}
		catch (RequestFailedException ex) when (ex.Status == 404)
		{
			return Error.NotFound();
		}
		catch (Exception ex)
		{
			return Error.Failure(ex.Message);
		}
	}

	public async Task<ErrorOr<Asset>> AddAsync(UserId userId, string name, InstrumentId instrumentId, CancellationToken cancellationToken)
	{
		var assetId = AssetId.New();

		var assetEntity = new AssetTableEntity
		{
			PartitionKey = userId.Value.ToString("N"),
			RowKey = assetId.Value.ToString("N"),
			Name = name,
			InstrumentId = instrumentId.Value.ToString("N"),
		};

		try
		{
			await this.tableClient.AddEntityAsync(assetEntity, cancellationToken: cancellationToken);
			return new Asset(assetId, name, instrumentId);
		}
		catch (RequestFailedException ex) when (ex.Status == 409)
		{
			return Error.Conflict();
		}
		catch (Exception ex)
		{
			return Error.Failure(ex.Message);
		}
	}

	public async Task<ErrorOr<Success>> DeleteAsync(UserId userId, AssetId assetId, CancellationToken cancellationToken)
	{
		try
		{
			await this.tableClient.DeleteEntityAsync(
				partitionKey: userId.Value.ToString("N"),
				rowKey: assetId.Value.ToString("N"),
				cancellationToken: cancellationToken);

			return Result.Success;
		}
		catch (RequestFailedException ex) when (ex.Status == 404)
		{
			return Error.NotFound();
		}
		catch (Exception ex)
		{
			return Error.Failure(description: ex.Message);
		}
	}

	private sealed class AssetTableEntity : ITableEntity
	{
		public string PartitionKey { get; set; }

		public string RowKey { get; set; }

		public DateTimeOffset? Timestamp { get; set; }

		public ETag ETag { get; set; }

		public string Name { get; set; }

		public string InstrumentId { get; set; }
	}
}
