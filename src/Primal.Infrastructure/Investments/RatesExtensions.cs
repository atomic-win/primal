namespace Primal.Infrastructure.Investments;

internal static class RatesExtensions
{
	internal static decimal GetOnOrBeforeValue(
		this IReadOnlyDictionary<DateOnly, decimal> rates,
		DateOnly date)
	{
		for (int lookback = 0; lookback < 7; ++lookback)
		{
			if (rates.TryGetValue(date.AddDays(-lookback), out var rate))
			{
				return rate;
			}
		}

		throw new InvalidOperationException(
			$"No rate found for date {date} or within the lookback period.");
	}

	internal static async Task<IReadOnlyDictionary<DateOnly, decimal>> GetOrFetchRatesAsync(
		this RateRepository rateRepository,
		string symbol,
		string rateType,
		Func<Task<IReadOnlyDictionary<DateOnly, decimal>>> fetchRates,
		CancellationToken cancellationToken)
	{
		var storedRates = await rateRepository.GetRecentRatesAsync(symbol, rateType, cancellationToken);
		if (storedRates.Count > 0)
		{
			return storedRates;
		}

		var rates = await fetchRates();
		await rateRepository.AddRatesAsync(symbol, rateType, rates, cancellationToken);
		return rates;
	}
}
