using Microsoft.Extensions.Caching.Hybrid;

using InvestmentPortfolioTracker.Core.Investments;

namespace InvestmentPortfolioTracker.Infrastructure.Investments;

internal sealed class CachedAssetApiClient<T> : IAssetApiClient<T>
{
	private readonly HybridCache cache;
	private readonly IAssetApiClient<T> assetApiClient;
	private readonly RateRepository rateRepository;
	private readonly RateType rateType;

	internal CachedAssetApiClient(
		HybridCache cache,
		IAssetApiClient<T> assetApiClient,
		RateRepository rateRepository,
		RateType rateType)
	{
		this.cache = cache;
		this.assetApiClient = assetApiClient;
		this.rateRepository = rateRepository;
		this.rateType = rateType;
	}

	public async Task<T> GetBySymbolAsync(string symbol, CancellationToken cancellationToken)
	{
		return await this.cache.GetOrCreateAsync(
			CacheKeyExtensions.AssetApiKey<T>(symbol),
			async entry => await this.assetApiClient.GetBySymbolAsync(symbol, cancellationToken),
			cancellationToken: cancellationToken);
	}

	public async Task<IReadOnlyDictionary<DateOnly, decimal>> GetPricesAsync(string symbol, CancellationToken cancellationToken)
	{
		return await this.cache.GetOrCreateAsync(
			CacheKeyExtensions.AssetApiPricesKey<T>(symbol),
			async entry => await this.rateRepository.GetOrFetchRatesAsync(
				symbol,
				this.rateType,
				async ct => await this.assetApiClient.GetPricesAsync(symbol, ct),
				cancellationToken),
			cancellationToken: cancellationToken);
	}

	public async Task<decimal> GetOnOrBeforePriceAsync(string symbol, DateOnly date, CancellationToken cancellationToken)
	{
		return await this.cache.GetOrCreateAsync(
			CacheKeyExtensions.AssetApiOnOrBeforePriceKey<T>(symbol, date),
			async entry => await this.GetOnOrBeforeValueAsyncInternal(symbol, date, cancellationToken),
			cancellationToken: cancellationToken);
	}

	private async Task<decimal> GetOnOrBeforeValueAsyncInternal(
		string symbol,
		DateOnly date,
		CancellationToken cancellationToken)
	{
		var prices = await this.GetPricesAsync(
			symbol,
			cancellationToken);

		return prices.GetOnOrBeforeValue(date);
	}
}
