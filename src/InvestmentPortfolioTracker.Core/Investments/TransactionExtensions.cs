namespace InvestmentPortfolioTracker.Core.Investments;

public static class TransactionExtensions
{
	public static IReadOnlyList<DateOnly> GetValuationDates(this TimeProvider timeProvider, DateOnly transactionDate)
	{
		var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
		var dates = new List<DateOnly> { today };

		var endOfMonth = new DateOnly(today.Year, today.Month, 1).AddDays(-1);

		while (endOfMonth >= transactionDate)
		{
			dates.Add(endOfMonth);
			endOfMonth = new DateOnly(endOfMonth.Year, endOfMonth.Month, 1).AddDays(-1);
		}

		return dates;
	}
}
