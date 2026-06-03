using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Hybrid;
using Primal.Application.Investments;
using Primal.Domain.Money;

namespace Primal.Infrastructure.Investments;

internal sealed class CachedForexApiClient : IForexApiClient
{
	private readonly HybridCache hybridCache;
	private readonly IForexApiClient forexApiClient;
	private readonly RateRepository rateRepository;

	internal CachedForexApiClient(
		HybridCache hybridCache,
		IForexApiClient forexApiClient,
		RateRepository rateRepository)
	{
		this.hybridCache = hybridCache;
		this.forexApiClient = forexApiClient;
		this.rateRepository = rateRepository;
	}

	public async Task<IReadOnlyDictionary<DateOnly, decimal>> GetForexRatesAsync(
		Currency fromCurrency,
		Currency toCurrency,
		CancellationToken cancellationToken)
	{
		if (fromCurrency == toCurrency)
		{
			return ImmutableDictionary<DateOnly, decimal>.Empty;
		}

		return await this.hybridCache.GetOrCreateAsync(
			$"forex/{fromCurrency}{toCurrency}/rates",
			async entry => await this.rateRepository.GetOrFetchRatesAsync(
				$"{fromCurrency}{toCurrency}",
				RateType.Forex,
				ct => this.forexApiClient.GetForexRatesAsync(fromCurrency, toCurrency, ct),
				cancellationToken),
			options: new HybridCacheEntryOptions
			{
				Flags = HybridCacheEntryFlags.None,
			},
			cancellationToken: cancellationToken);
	}

	public async Task<decimal> GetOnOrBeforeForexRateAsync(
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
			$"forex/{fromCurrency}{toCurrency}/rates/{date:yyyy-MM-dd}/on-or-before",
			async entry => await this.GetOnOrBeforeForexRateInternalAsync(fromCurrency, toCurrency, date, cancellationToken),
			cancellationToken: cancellationToken);
	}

	private async Task<decimal> GetOnOrBeforeForexRateInternalAsync(
		Currency fromCurrency,
		Currency toCurrency,
		DateOnly date,
		CancellationToken cancellationToken)
	{
		var rates = await this.GetForexRatesAsync(
			fromCurrency,
			toCurrency,
			cancellationToken);

		return rates.GetOnOrBeforeValue(date);
	}
}
