using Microsoft.EntityFrameworkCore;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Infrastructure.Persistence;

namespace Primal.Infrastructure.Investments;

internal sealed class AssetRepository : IAssetRepository
{
	private readonly AppDbContext appDbContext;

	internal AssetRepository(AppDbContext appDbContext)
	{
		this.appDbContext = appDbContext;
	}

	public async Task<Asset> GetByIdAsync(AssetId assetId, CancellationToken cancellationToken)
	{
		var assetTableEntity = await this.appDbContext.Assets
			.AsNoTracking()
			.FirstOrDefaultAsync(a => a.Id == assetId.Value, cancellationToken);

		if (assetTableEntity is null)
		{
			return Asset.Empty;
		}

		return this.MapToAsset(assetTableEntity);
	}

	public async Task<Asset> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken)
	{
		var assetTableEntity = await this.appDbContext.Assets
			.AsNoTracking()
			.FirstOrDefaultAsync(a => a.ExternalId == externalId, cancellationToken);

		if (assetTableEntity is null)
		{
			return Asset.Empty;
		}

		return this.MapToAsset(assetTableEntity);
	}

	public async Task<Asset> AddAsync(
		string name,
		AssetClass assetClass,
		AssetType assetType,
		Currency currency,
		string externalId,
		CancellationToken cancellationToken)
	{
		var assetTableEntity = new AssetTableEntity
		{
			Id = Guid.CreateVersion7(),
			Name = name,
			AssetClass = assetClass,
			AssetType = assetType,
			Currency = currency,
			ExternalId = externalId,
		};

		await this.appDbContext.Assets.AddAsync(assetTableEntity, cancellationToken);

		return this.MapToAsset(assetTableEntity);
	}

	private Asset MapToAsset(AssetTableEntity assetTableEntity)
	{
		return new Asset(
			new AssetId(assetTableEntity.Id),
			assetTableEntity.Name,
			assetTableEntity.AssetClass,
			assetTableEntity.AssetType,
			assetTableEntity.Currency,
			assetTableEntity.ExternalId);
	}
}
