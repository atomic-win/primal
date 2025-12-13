using System.Collections.Concurrent;
using System.Collections.Immutable;
using Primal.Application.Investments;
using Primal.Domain.Money;

namespace Primal.Infrastructure.Investments;

internal sealed class CachedExchangeRateApiClient : IExchangeRateApiClient
{
	private readonly IExchangeRateApiClient exchangeRateProvider;

	private readonly ConcurrentDictionary<(Currency From, Currency To), Lazy<Task<IReadOnlyDictionary<DateOnly, decimal>>>> exchangeRatesCache
		= new ConcurrentDictionary<(Currency From, Currency To), Lazy<Task<IReadOnlyDictionary<DateOnly, decimal>>>>();

	private readonly ConcurrentDictionary<(Currency From, Currency To, DateOnly Date), Lazy<Task<decimal>>> onOrBeforeExchangeRateCache
		= new ConcurrentDictionary<(Currency From, Currency To, DateOnly Date), Lazy<Task<decimal>>>();

	internal CachedExchangeRateApiClient(IExchangeRateApiClient exchangeRateProvider)
	{
		this.exchangeRateProvider = exchangeRateProvider;
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

		var key = (From: fromCurrency, To: toCurrency);
		var lazyResult = this.exchangeRatesCache.GetOrAdd(key, k => new Lazy<Task<IReadOnlyDictionary<DateOnly, decimal>>>(() => this.exchangeRateProvider.GetExchangeRatesAsync(k.From, k.To, cancellationToken)));
		return await lazyResult.Value;
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

		var key = (From: fromCurrency, To: toCurrency, Date: date);
		var lazyResult = this.onOrBeforeExchangeRateCache.GetOrAdd(key, k => new Lazy<Task<decimal>>(() => this.GetOnOrBeforeExchangeRateInternalAsync(k.From, k.To, k.Date, cancellationToken)));
		return await lazyResult.Value;
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
