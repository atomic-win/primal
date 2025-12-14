using Microsoft.Extensions.Caching.Hybrid;
using Primal.Application.Investments;

namespace Primal.Infrastructure.Investments;

internal sealed class CachedAssetApiClient<T> : IAssetApiClient<T>
{
	private readonly HybridCache hybridCache;
	private readonly IAssetApiClient<T> assetApiClient;

	internal CachedAssetApiClient(
		HybridCache hybridCache,
		IAssetApiClient<T> assetApiClient)
	{
		this.hybridCache = hybridCache;
		this.assetApiClient = assetApiClient;
	}

	public async Task<T> GetBySymbolAsync(string symbol, CancellationToken cancellationToken)
	{
		return await this.hybridCache.GetOrCreateAsync(
			$"asset/{typeof(T).Name}/{symbol}",
			async entry => await this.assetApiClient.GetBySymbolAsync(symbol, cancellationToken),
			cancellationToken: cancellationToken);
	}

	public async Task<IReadOnlyDictionary<DateOnly, decimal>> GetPricesAsync(string symbol, CancellationToken cancellationToken)
	{
		return await this.hybridCache.GetOrCreateAsync(
			$"asset/{typeof(T).Name}/{symbol}/prices",
			async entry => await this.assetApiClient.GetPricesAsync(symbol, cancellationToken),
			cancellationToken: cancellationToken);
	}

	public async Task<decimal> GetOnOrBeforePriceAsync(string symbol, DateOnly date, CancellationToken cancellationToken)
	{
		return await this.hybridCache.GetOrCreateAsync(
			$"asset/{typeof(T).Name}/{symbol}/prices/{date:yyyy-MM-dd}/on-or-before",
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
