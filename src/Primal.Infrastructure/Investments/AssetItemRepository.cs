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

	private AssetItem MapToAssetItem(AssetItemTableEntity assetItemTableEntity)
	{
		return new AssetItem(
			new AssetItemId(assetItemTableEntity.Id),
			new AssetId(assetItemTableEntity.AssetId),
			assetItemTableEntity.Name);
	}
}
