using System.Collections.Immutable;
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

	internal static decimal CalculateInvestedValue(
		this IEnumerable<Transaction> transactions,
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates,
		DateOnly evaluationDate)
	{
		transactions = transactions.OrderByDescending(x => x.Date).ToImmutableArray();

		decimal withdrawnCashUnits = transactions.Select(transaction => transaction switch
		{
			{ Type: TransactionType.Withdrawal or TransactionType.InterestPenalty } => transaction.Units,
			_ => 0m,
		}).Sum();

		decimal withdrawnNonCashUnits = transactions.Select(transaction => transaction switch
		{
			{ Type: TransactionType.Sell } => transaction.Units,
			_ => 0m,
		}).Sum();

		decimal investedValue = 0;

		foreach (var transaction in transactions)
		{
			switch (transaction.Type)
			{
				case TransactionType.Deposit:
					{
						var transactionUnits = transaction.Units - Math.Min(withdrawnCashUnits, transaction.Units);
						withdrawnCashUnits -= transaction.Units - transactionUnits;

						var balanceUnitsTransaction = new Transaction(transaction.Id, transaction.Date, transaction.Name, transaction.Type, transaction.AssetId, transactionUnits);
						investedValue += balanceUnitsTransaction.CalculateInitialAmount(historicalPrices, historicalExchangeRates, evaluationDate);
						break;
					}

				case TransactionType.Buy:
					{
						var transactionUnits = transaction.Units - Math.Min(withdrawnNonCashUnits, transaction.Units);
						withdrawnNonCashUnits -= transaction.Units - transactionUnits;

						var balanceUnitsTransaction = new Transaction(transaction.Id, transaction.Date, transaction.Name, transaction.Type, transaction.AssetId, transactionUnits);
						investedValue += balanceUnitsTransaction.CalculateInitialAmount(historicalPrices, historicalExchangeRates, evaluationDate);
						break;
					}

				default:
					break;
			}
		}

		return investedValue;
	}

	internal static decimal CalculateCurrentValue(
		this IEnumerable<Transaction> transactions,
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates,
		DateOnly evaluationDate)
	{
		return transactions.Sum(transaction =>
		{
			switch (transaction.Type)
			{
				case TransactionType.Buy:
				case TransactionType.Deposit:
				case TransactionType.SelfInterest:
					return transaction.CalculateCurrentAmount(historicalPrices, historicalExchangeRates, evaluationDate);
				case TransactionType.Sell:
					return -transaction.CalculateInitialAmount(historicalPrices, historicalExchangeRates, evaluationDate);
				case TransactionType.Withdrawal:
				case TransactionType.InterestPenalty:
					return -transaction.CalculateCurrentAmount(historicalPrices, historicalExchangeRates, evaluationDate);
				default:
					return 0;
			}
		});
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

	private static decimal CalculateInitialAmount(
		this Transaction transaction,
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates,
		DateOnly evaluationDate)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThan(transaction.Date, evaluationDate);

		return transaction.CalculateAmount(historicalPrices.GetHistoricalValue(transaction.Date), historicalExchangeRates.GetHistoricalValue(transaction.Date));
	}

	private static decimal CalculateCurrentAmount(
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
