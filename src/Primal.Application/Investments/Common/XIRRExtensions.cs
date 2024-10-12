using System.Collections.Immutable;

namespace Primal.Application.Investments;

internal static class XIRRExtensions
{
	internal static decimal CalculateXIRR(this IEnumerable<(decimal Years, decimal TransactionAmount, decimal BalanceAmount)> inputs)
	{
		IReadOnlyList<(decimal Years, decimal TransactionAmount, decimal BalanceAmount)> inputsList
			= inputs.ToImmutableArray();

		decimal outValue = inputsList.Sum(x => x.BalanceAmount);
		var inValues = inputsList.Select(x => (x.Years, x.TransactionAmount)).ToImmutableArray();

		return CalculateXIRR(inValues, outValue);
	}

	private static decimal CalculateXIRR(IReadOnlyList<(decimal Years, decimal Amount)> inValues, decimal outValue)
	{
		bool allLessThanYear = inValues.All(x => x.Years < 1);

		if (allLessThanYear && outValue != 0)
		{
			inValues = inValues.Select(x => (1M, x.Amount)).ToImmutableArray();
		}

		decimal rateLowerBound = -1, rateUpperBound = 100;
		while (rateUpperBound - rateLowerBound > 0.0000001M)
		{
			decimal rateMiddle = (rateLowerBound + rateUpperBound) / 2;
			decimal value = inValues.CalculateValue(rateMiddle);
			if (value > outValue)
			{
				rateUpperBound = rateMiddle;
			}
			else
			{
				rateLowerBound = rateMiddle;
			}
		}

		return (rateLowerBound + rateUpperBound) / 2;
	}

	private static decimal CalculateValue(
		this IReadOnlyList<(decimal Years, decimal Amount)> inValues,
		decimal rate)
	{
		return inValues
			.Sum(x => (decimal)Math.Pow((double)(1 + rate), (double)x.Years) * x.Amount);
	}
}
