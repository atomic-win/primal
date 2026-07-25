using Microsoft.Extensions.Caching.Hybrid;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Infrastructure.Investments;

internal sealed class CachedAssetRepository : IAssetRepository
{
	private readonly HybridCache cache;
	private readonly IAssetRepository assetRepository;

	internal CachedAssetRepository(
		HybridCache cache,
		IAssetRepository assetRepository)
	{
		this.cache = cache;
		this.assetRepository = assetRepository;
	}

	public async Task<Asset> GetByIdAsync(
		AssetId assetId,
		CancellationToken cancellationToken)
	{
		return await this.cache.GetOrCreateAsync(
			assetId.AssetKey(),
			async entry => await this.assetRepository.GetByIdAsync(assetId, cancellationToken),
			cancellationToken: cancellationToken);
	}

	public async Task<Asset> GetByExternalIdAsync(
		string externalId,
		CancellationToken cancellationToken)
	{
		return await this.cache.GetOrCreateAsync(
			CacheKeyExtensions.AssetByExternalIdKey(externalId),
			async entry => await this.assetRepository.GetByExternalIdAsync(externalId, cancellationToken),
			cancellationToken: cancellationToken);
	}

	public async Task<Asset> AddAsync(
		string name,
		AssetClass assetClass,
		AssetType assetType,
		Currency currency,
		string externalId,
		CancellationToken cancellationToken)
	{
		var asset = await this.assetRepository.AddAsync(
			name,
			assetClass,
			assetType,
			currency,
			externalId,
			cancellationToken);

		await this.cache.RemoveAsync(
			CacheKeyExtensions.AssetByExternalIdKey(externalId),
			cancellationToken: cancellationToken);

		return asset;
	}
}
