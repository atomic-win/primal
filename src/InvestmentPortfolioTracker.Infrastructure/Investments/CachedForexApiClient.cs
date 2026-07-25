using System.Collections.Immutable;
using InvestmentPortfolioTracker.Core.Investments;
using InvestmentPortfolioTracker.Domain.Money;
using Microsoft.Extensions.Caching.Hybrid;

namespace InvestmentPortfolioTracker.Infrastructure.Investments;

internal sealed class CachedForexApiClient : IForexApiClient
{
	private readonly HybridCache cache;
	private readonly IForexApiClient forexApiClient;
	private readonly RateRepository rateRepository;

	internal CachedForexApiClient(
		HybridCache cache,
		IForexApiClient forexApiClient,
		RateRepository rateRepository)
	{
		this.cache = cache;
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

		return await this.cache.GetOrCreateAsync(
			CacheKeyExtensions.ForexRatesKey(fromCurrency, toCurrency),
			async entry => await this.rateRepository.GetOrFetchRatesAsync(
				$"{fromCurrency}{toCurrency}",
				RateType.Forex,
				async ct => await this.forexApiClient.GetForexRatesAsync(fromCurrency, toCurrency, ct),
				cancellationToken),
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

		return await this.cache.GetOrCreateAsync(
			CacheKeyExtensions.ForexOnOrBeforeRateKey(fromCurrency, toCurrency, date),
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
