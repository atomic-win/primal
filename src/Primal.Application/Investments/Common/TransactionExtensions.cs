using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal static class TransactionExtensions
{
	internal static decimal CalculateTransactionAmount(
		this Transaction transaction,
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates)
	{
		return transaction.CalculateInitialAmount(historicalPrices, historicalExchangeRates, transaction.Date);
	}

	internal static decimal CalculateXIRRTransactionAmount(
		this Transaction transaction,
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates,
		DateOnly evaluationDate)
	{
		switch (transaction.Type)
		{
			case TransactionType.Buy:
			case TransactionType.Deposit:
			case TransactionType.InterestPenalty:
				return transaction.CalculateInitialAmount(historicalPrices, historicalExchangeRates, evaluationDate);
			case TransactionType.Sell:
			case TransactionType.Withdrawal:
			case TransactionType.Interest:
			case TransactionType.Dividend:
				return -transaction.CalculateInitialAmount(historicalPrices, historicalExchangeRates, evaluationDate);
			default:
				return 0;
		}
	}

	internal static decimal CalculateXIRRBalanceAmount(
		this Transaction transaction,
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates,
		DateOnly evaluationDate)
	{
		switch (transaction.Type)
		{
			case TransactionType.Buy:
			case TransactionType.Deposit:
			case TransactionType.SelfInterest:
				return transaction.CalculateCurrentAmount(historicalPrices, historicalExchangeRates, evaluationDate);
			case TransactionType.Sell:
			case TransactionType.Withdrawal:
			case TransactionType.InterestPenalty:
				return -transaction.CalculateCurrentAmount(historicalPrices, historicalExchangeRates, evaluationDate);
			default:
				return 0;
		}
	}

	internal static decimal CalculateInitialAmount(
		this Transaction transaction,
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates,
		DateOnly evaluationDate)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThan(transaction.Date, evaluationDate);

		return transaction.CalculateAmount(historicalPrices.GetHistoricalValue(transaction.Date), historicalExchangeRates.GetHistoricalValue(transaction.Date));
	}

	internal static decimal CalculateCurrentAmount(
		this Transaction transaction,
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates,
		DateOnly evaluationDate)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThan(transaction.Date, evaluationDate);

		return transaction.CalculateAmount(historicalPrices.GetHistoricalValue(evaluationDate), historicalExchangeRates.GetHistoricalValue(evaluationDate));
	}

	private static decimal CalculateAmount(
		this Transaction transaction,
		decimal price,
		decimal exchangeRate)
	{
		decimal amount = transaction.Type switch
		{
			TransactionType.Dividend => transaction.Units,
			_ => transaction.Units * price,
		};

		return exchangeRate * amount;
	}

	private static decimal GetHistoricalValue(
		this IReadOnlyDictionary<DateOnly, decimal> historicalValues,
		DateOnly date)
	{
		if (historicalValues.Count == 0)
		{
			return 1;
		}

		decimal value;
		while (!historicalValues.TryGetValue(date, out value))
		{
			date = date.AddDays(-1);
		}

		return value;
	}
}
