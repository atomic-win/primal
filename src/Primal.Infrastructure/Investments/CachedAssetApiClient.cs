using System.Collections.Concurrent;
using Primal.Application.Investments;

namespace Primal.Infrastructure.Investments;

internal sealed class CachedAssetApiClient<T> : IAssetApiClient<T>
{
	private readonly IAssetApiClient<T> assetApiClient;

	private readonly ConcurrentDictionary<string, Lazy<Task<T>>> symbolCache
		= new ConcurrentDictionary<string, Lazy<Task<T>>>(StringComparer.OrdinalIgnoreCase);

	private readonly ConcurrentDictionary<string, Lazy<Task<IReadOnlyDictionary<DateOnly, decimal>>>> pricesCache
		= new ConcurrentDictionary<string, Lazy<Task<IReadOnlyDictionary<DateOnly, decimal>>>>(StringComparer.OrdinalIgnoreCase);

	private readonly ConcurrentDictionary<(string Symbol, DateOnly Date), Lazy<Task<decimal>>> onOrBeforePriceCache
		= new ConcurrentDictionary<(string Symbol, DateOnly Date), Lazy<Task<decimal>>>();

	internal CachedAssetApiClient(IAssetApiClient<T> assetApiClient)
	{
		this.assetApiClient = assetApiClient;
	}

	public async Task<T> GetBySymbolAsync(string symbol, CancellationToken cancellationToken)
	{
		var lazyResult = this.symbolCache.GetOrAdd(symbol, s => new Lazy<Task<T>>(() => this.assetApiClient.GetBySymbolAsync(s, cancellationToken)));
		return await lazyResult.Value;
	}

	public async Task<IReadOnlyDictionary<DateOnly, decimal>> GetPricesAsync(string symbol, CancellationToken cancellationToken)
	{
		var lazyResult = this.pricesCache.GetOrAdd(symbol, s => new Lazy<Task<IReadOnlyDictionary<DateOnly, decimal>>>(() => this.assetApiClient.GetPricesAsync(s, cancellationToken)));
		return await lazyResult.Value;
	}

	public async Task<decimal> GetOnOrBeforePriceAsync(string symbol, DateOnly date, CancellationToken cancellationToken)
	{
		var key = (Symbol: symbol, Date: date);
		var lazyResult = this.onOrBeforePriceCache.GetOrAdd(key, k => new Lazy<Task<decimal>>(() => this.GetOnOrBeforeValueAsyncInternal(k.Symbol, k.Date, cancellationToken)));
		return await lazyResult.Value;
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
