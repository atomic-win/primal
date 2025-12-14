using Microsoft.Extensions.Caching.Hybrid;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Infrastructure.Investments;

internal sealed class CachedAssetRepository : IAssetRepository
{
	private readonly HybridCache hybridCache;
	private readonly IAssetRepository assetRepository;

	internal CachedAssetRepository(
		HybridCache hybridCache,
		IAssetRepository assetRepository)
	{
		this.hybridCache = hybridCache;
		this.assetRepository = assetRepository;
	}

	public async Task<Asset> GetByIdAsync(
		AssetId assetId,
		CancellationToken cancellationToken)
	{
		return await this.hybridCache.GetOrCreateAsync(
			$"assets/{assetId.Value}",
			async entry => await this.assetRepository.GetByIdAsync(assetId, cancellationToken),
			cancellationToken: cancellationToken);
	}

	public async Task<Asset> GetByExternalIdAsync(
		string externalId,
		CancellationToken cancellationToken)
	{
		return await this.hybridCache.GetOrCreateAsync(
			$"assets/external/{externalId}",
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
		return await this.assetRepository.AddAsync(
			name,
			assetClass,
			assetType,
			currency,
			externalId,
			cancellationToken);
	}
}
