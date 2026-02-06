using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Hybrid;
using Primal.Application.Investments;
using Primal.Domain.Money;

namespace Primal.Infrastructure.Investments;

internal sealed class CachedExchangeRateApiClient : IExchangeRateApiClient
{
	private readonly HybridCache hybridCache;
	private readonly IExchangeRateApiClient exchangeRateApiClient;

	internal CachedExchangeRateApiClient(
		HybridCache hybridCache,
		IExchangeRateApiClient exchangeRateApiClient)
	{
		this.hybridCache = hybridCache;
		this.exchangeRateApiClient = exchangeRateApiClient;
	}

	public async Task<IReadOnlyDictionary<DateOnly, decimal>> GetExchangeRatesAsync(
		Currency fromCurrency,
		Currency toCurrency,
		CancellationToken cancellationToken)
	{
		if (fromCurrency == toCurrency)
		{
			return ImmutableDictionary<DateOnly, decimal>.Empty;
		}

		return await this.hybridCache.GetOrCreateAsync(
			$"exchange-rate/{fromCurrency}/{toCurrency}/rates",
			async entry => await this.exchangeRateApiClient.GetExchangeRatesAsync(fromCurrency, toCurrency, cancellationToken),
			options: new HybridCacheEntryOptions
			{
				Flags = HybridCacheEntryFlags.None,
			},
			cancellationToken: cancellationToken);
	}

	public async Task<decimal> GetOnOrBeforeExchangeRateAsync(
		Currency fromCurrency,
		Currency toCurrency,
		DateOnly date,
		CancellationToken cancellationToken)
	{
		if (fromCurrency == toCurrency)
		{
			return 1m;
		}

		return await this.hybridCache.GetOrCreateAsync(
			$"exchange-rate/{fromCurrency}/{toCurrency}/rates/{date:yyyy-MM-dd}/on-or-before",
			async entry => await this.GetOnOrBeforeExchangeRateInternalAsync(fromCurrency, toCurrency, date, cancellationToken),
			cancellationToken: cancellationToken);
	}

	private async Task<decimal> GetOnOrBeforeExchangeRateInternalAsync(
		Currency fromCurrency,
		Currency toCurrency,
		DateOnly date,
		CancellationToken cancellationToken)
	{
		var rates = await this.GetExchangeRatesAsync(
			fromCurrency,
			toCurrency,
			cancellationToken);

		return rates.GetOnOrBeforeValue(date);
	}
}
