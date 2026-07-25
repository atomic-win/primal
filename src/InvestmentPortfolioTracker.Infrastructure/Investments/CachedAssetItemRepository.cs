using Microsoft.Extensions.Caching.Hybrid;

using InvestmentPortfolioTracker.Core.Investments;
using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Infrastructure.Investments;

internal sealed class CachedAssetItemRepository : IAssetItemRepository
{
	private readonly HybridCache cache;
	private readonly IAssetItemRepository assetItemRepository;

	internal CachedAssetItemRepository(
		HybridCache cache,
		IAssetItemRepository assetItemRepository)
	{
		this.cache = cache;
		this.assetItemRepository = assetItemRepository;
	}

	public async Task<IEnumerable<AssetItem>> GetAllAsync(
		UserId userId,
		CancellationToken cancellationToken)
	{
		return await this.cache.GetOrCreateAsync(
			userId.AssetItemsKey(),
			async entry => await this.assetItemRepository.GetAllAsync(userId, cancellationToken),
			cancellationToken: cancellationToken);
	}

	public async Task<AssetItem> GetByIdAsync(
		UserId userId,
		AssetItemId assetItemId,
		CancellationToken cancellationToken)
	{
		return await this.cache.GetOrCreateAsync(
			userId.AssetItemKey(assetItemId),
			async entry => await this.assetItemRepository.GetByIdAsync(userId, assetItemId, cancellationToken),
			cancellationToken: cancellationToken);
	}

	public async Task<AssetItem> AddAsync(
		UserId userId,
		AssetId assetId,
		string name,
		CancellationToken cancellationToken)
	{
		var assetItem = await this.assetItemRepository.AddAsync(
			userId,
			assetId,
			name,
			cancellationToken);

		await this.cache.RemoveAsync(
			userId.AssetItemsKey(),
			cancellationToken: cancellationToken);

		return assetItem;
	}

	public async Task UpdateAsync(
		UserId userId,
		AssetItem assetItem,
		CancellationToken cancellationToken)
	{
		await this.assetItemRepository.UpdateAsync(
			userId,
			assetItem,
			cancellationToken);

		await this.cache.RemoveAsync(
			userId.AssetItemsKey(),
			cancellationToken: cancellationToken);

		await this.cache.RemoveAsync(
			userId.AssetItemKey(assetItem.Id),
			cancellationToken: cancellationToken);
	}

	public async Task DeleteAsync(
		UserId userId,
		AssetItemId assetItemId,
		CancellationToken cancellationToken)
	{
		await this.assetItemRepository.DeleteAsync(
			userId,
			assetItemId,
			cancellationToken);

		await this.cache.RemoveAsync(
			userId.AssetItemsKey(),
			cancellationToken: cancellationToken);

		await this.cache.RemoveAsync(
			userId.AssetItemKey(assetItemId),
			cancellationToken: cancellationToken);
	}
}
