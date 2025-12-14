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
}
