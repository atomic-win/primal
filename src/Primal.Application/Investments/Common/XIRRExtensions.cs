using System.Collections.Immutable;

namespace Primal.Application.Investments;

internal static class XIRRExtensions
{
	internal static decimal CalculateXIRR(this IEnumerable<(decimal Years, decimal TransactionAmount, decimal BalanceAmount)> inputs)
	{
		IReadOnlyList<(decimal Years, decimal TransactionAmount, decimal BalanceAmount)> inputsList
			= inputs.ToImmutableArray();

		if (inputsList.Sum(x => x.BalanceAmount - x.TransactionAmount) == 0)
		{
			return 0;
		}

		decimal rateLowerBound = 0, rateUpperBound = 100;
		while (rateUpperBound - rateLowerBound > 0.0000001M)
		{
			decimal rateMiddle = (rateLowerBound + rateUpperBound) / 2;
			decimal value = inputsList.CalculateValue(rateMiddle);
			if (value > 0)
			{
				rateLowerBound = rateMiddle;
			}
			else
			{
				rateUpperBound = rateMiddle;
			}
		}

		return (rateLowerBound + rateUpperBound) / 2;
	}

	private static decimal CalculateValue(
		this IReadOnlyList<(decimal Years, decimal TransactionAmount, decimal BalanceAmount)> inputs,
		decimal rate)
	{
		return inputs
			.Sum(x => x.BalanceAmount - ((decimal)Math.Pow((double)(1 + rate), (double)Math.Max(1M, x.Years)) * x.TransactionAmount));
	}
}
