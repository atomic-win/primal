using Microsoft.Extensions.Caching.Hybrid;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Investments;

internal sealed class CachedAssetItemRepository : IAssetItemRepository
{
	private readonly HybridCache hybridCache;
	private readonly IAssetItemRepository assetItemRepository;

	internal CachedAssetItemRepository(
		HybridCache hybridCache,
		IAssetItemRepository assetItemRepository)
	{
		this.hybridCache = hybridCache;
		this.assetItemRepository = assetItemRepository;
	}

	public async Task<IEnumerable<AssetItem>> GetAllAsync(
		UserId userId,
		CancellationToken cancellationToken)
	{
		return await this.hybridCache.GetOrCreateAsync(
			$"users/{userId.Value}/assetItems",
			async entry => await this.assetItemRepository.GetAllAsync(userId, cancellationToken),
			cancellationToken: cancellationToken);
	}

	public async Task<AssetItem> GetByIdAsync(
		UserId userId,
		AssetItemId assetItemId,
		CancellationToken cancellationToken)
	{
		return await this.hybridCache.GetOrCreateAsync(
			$"users/{userId.Value}/assetItems/{assetItemId.Value}",
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

		await this.hybridCache.RemoveAsync(
			$"users/{userId.Value}/assetItems",
			cancellationToken: cancellationToken);

		return assetItem;
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

		await this.hybridCache.RemoveAsync(
			$"users/{userId.Value}/assetItems",
			cancellationToken: cancellationToken);

		await this.hybridCache.RemoveAsync(
			$"users/{userId.Value}/assetItems/{assetItemId.Value}",
			cancellationToken: cancellationToken);
	}
}
