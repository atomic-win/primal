using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal static class TransactionExtensions
{
	internal static decimal CalculateTransactionAmount(
		this Transaction transaction,
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates)
	{
		return transaction.CalculateInitialAmount(historicalPrices, historicalExchangeRates);
	}

	internal static decimal CalculateInitialBalanceAmount(
		this Transaction transaction,
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates)
	{
		switch (transaction.Type)
		{
			case TransactionType.Buy:
				return transaction.CalculateInitialAmount(historicalPrices, historicalExchangeRates);
			case TransactionType.Deposit:
			case TransactionType.SelfInterest:
				return transaction.CalculateCurrentAmount(historicalPrices, historicalExchangeRates);
			case TransactionType.Sell:
				return -transaction.CalculateInitialAmount(historicalPrices, historicalExchangeRates);
			case TransactionType.Withdrawal:
			case TransactionType.InterestPenalty:
				return -transaction.CalculateCurrentAmount(historicalPrices, historicalExchangeRates);
			default:
				return 0;
		}
	}

	internal static decimal CalculateCurrentBalanceAmount(
		this Transaction transaction,
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates)
	{
		switch (transaction.Type)
		{
			case TransactionType.Buy:
			case TransactionType.Deposit:
			case TransactionType.SelfInterest:
				return transaction.CalculateCurrentAmount(historicalPrices, historicalExchangeRates);
			case TransactionType.Sell:
				return -transaction.CalculateInitialAmount(historicalPrices, historicalExchangeRates);
			case TransactionType.Withdrawal:
			case TransactionType.InterestPenalty:
				return -transaction.CalculateCurrentAmount(historicalPrices, historicalExchangeRates);
			default:
				return 0;
		}
	}

	internal static decimal CalculateXIRRTransactionAmount(
		this Transaction transaction,
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates)
	{
		switch (transaction.Type)
		{
			case TransactionType.Buy:
			case TransactionType.Deposit:
				return transaction.CalculateInitialAmount(historicalPrices, historicalExchangeRates);
			case TransactionType.Sell:
			case TransactionType.Withdrawal:
				return -transaction.CalculateInitialAmount(historicalPrices, historicalExchangeRates);
			default:
				return 0;
		}
	}

	internal static decimal CalculateXIRRBalanceAmount(
		this Transaction transaction,
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates)
	{
		switch (transaction.Type)
		{
			case TransactionType.Buy:
				return transaction.CalculateCurrentAmount(historicalPrices, historicalExchangeRates);
			case TransactionType.Deposit:
			case TransactionType.SelfInterest:
			case TransactionType.Dividend:
			case TransactionType.Interest:
				return transaction.CalculateInitialAmount(historicalPrices, historicalExchangeRates);
			case TransactionType.Sell:
			case TransactionType.Withdrawal:
			case TransactionType.InterestPenalty:
				return -transaction.CalculateInitialAmount(historicalPrices, historicalExchangeRates);
			default:
				return 0;
		}
	}

	private static decimal CalculateInitialAmount(
		this Transaction transaction,
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates)
	{
		return transaction.CalculateAmount(
			historicalPrices.GetHistoricalValue(transaction.Date),
			historicalExchangeRates.GetHistoricalValue(transaction.Date));
	}

	private static decimal CalculateCurrentAmount(
		this Transaction transaction,
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates)
	{
		DateOnly date = DateOnly.FromDateTime(DateTime.UtcNow);

		return transaction.CalculateAmount(
			historicalPrices.GetHistoricalValue(date),
			historicalExchangeRates.GetHistoricalValue(date));
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
