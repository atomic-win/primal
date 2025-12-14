using Microsoft.EntityFrameworkCore;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;
using Primal.Infrastructure.Persistence;

namespace Primal.Infrastructure.Investments;

internal sealed class AssetItemRepository : IAssetItemRepository
{
	private readonly AppDbContext appDbContext;

	internal AssetItemRepository(AppDbContext appDbContext)
	{
		this.appDbContext = appDbContext;
	}

	public async Task<IEnumerable<AssetItem>> GetAllAsync(
		UserId userId,
		CancellationToken cancellationToken)
	{
		var assetItemTableEntities = await this.appDbContext.AssetItems
			.Where(ai => ai.UserId == userId.Value)
			.ToListAsync(cancellationToken);

		return assetItemTableEntities.Select(this.MapToAssetItem);
	}

	public async Task<AssetItem> GetByIdAsync(
		UserId userId,
		AssetItemId assetItemId,
		CancellationToken cancellationToken)
	{
		var assetItemTableEntity = await this.appDbContext.AssetItems
			.FirstOrDefaultAsync(ai => ai.UserId == userId.Value && ai.Id == assetItemId.Value, cancellationToken);
		if (assetItemTableEntity is null)
		{
			return AssetItem.Empty;
		}

		return this.MapToAssetItem(assetItemTableEntity);
	}

	public async Task<AssetItem> AddAsync(
		UserId userId,
		AssetId assetId,
		string name,
		CancellationToken cancellationToken)
	{
		var assetItemTableEntity = new AssetItemTableEntity
		{
			Id = Guid.CreateVersion7(),
			UserId = userId.Value,
			AssetId = assetId.Value,
			Name = name,
		};

		await this.appDbContext.AssetItems.AddAsync(assetItemTableEntity, cancellationToken);

		return this.MapToAssetItem(assetItemTableEntity);
	}

	public async Task DeleteAsync(
		UserId userId,
		AssetItemId assetItemId,
		CancellationToken cancellationToken)
	{
		await this.appDbContext.AssetItems
			.Where(ai => ai.UserId == userId.Value && ai.Id == assetItemId.Value)
			.ExecuteDeleteAsync(cancellationToken);
	}

	private AssetItem MapToAssetItem(AssetItemTableEntity assetItemTableEntity)
	{
		return new AssetItem(
			new AssetItemId(assetItemTableEntity.Id),
			new AssetId(assetItemTableEntity.AssetId),
			assetItemTableEntity.Name);
	}
}
