using System.Collections.Frozen;
using System.Collections.Immutable;
using ErrorOr;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

internal sealed class InvestmentCalculator
{
	private readonly ITransactionRepository transactionRepository;
	private readonly IAssetRepository assetRepository;
	private readonly IInstrumentRepository instrumentRepository;

	private readonly IMutualFundApiClient mutualFundApiClient;
	private readonly IStockApiClient stockApiClient;
	private readonly IExchangeRateProvider exchangeRateProvider;

	public InvestmentCalculator(
		ITransactionRepository transactionRepository,
		IAssetRepository assetRepository,
		IInstrumentRepository instrumentRepository,
		IMutualFundApiClient mutualFundApiClient,
		IStockApiClient stockApiClient,
		IExchangeRateProvider exchangeRateProvider)
	{
		this.transactionRepository = transactionRepository;
		this.assetRepository = assetRepository;
		this.instrumentRepository = instrumentRepository;
		this.mutualFundApiClient = mutualFundApiClient;
		this.stockApiClient = stockApiClient;
		this.exchangeRateProvider = exchangeRateProvider;
	}

	internal async Task<ErrorOr<ValuationResult>> CalculateValuationAsync(
		UserId userId,
		DateOnly evaluationDate,
		IEnumerable<AssetId> assetIds,
		Currency currency,
		CancellationToken cancellationToken)
	{
		var errorOrAssets = await this.assetRepository.GetAllAsync(userId, cancellationToken);

		if (errorOrAssets.IsError)
		{
			return errorOrAssets.Errors;
		}

		var assets = errorOrAssets.Value.Where(x => assetIds.Contains(x.Id));

		foreach (var assetId in assetIds)
		{
			if (!assets.Any(x => x.Id == assetId))
			{
				return Error.NotFound(description: $"Asset with ID '{assetId}' not found.");
			}
		}

		var transactions = new List<Transaction>();

		foreach (var assetId in assetIds)
		{
			var errorOrTransactions = await this.transactionRepository.GetByAssetIdAsync(userId, assetId, cancellationToken);

			if (errorOrTransactions.IsError)
			{
				return errorOrTransactions.Errors;
			}

			transactions.AddRange(errorOrTransactions.Value.Where(x => x.Date <= evaluationDate));
		}

		return await this.CalculateAsync(
			currency,
			transactions,
			assets,
			(assetMap, instrumentMap, historicalPricesMap, historicalExchangeRatesMap, transactions) =>
				this.CalculateValuation(evaluationDate, assetMap, instrumentMap, historicalPricesMap, historicalExchangeRatesMap, transactions),
			cancellationToken);
	}

	internal async Task<ErrorOr<IEnumerable<TransactionResult>>> CalculateTransactionResultsAsync(
		UserId userId,
		Currency currency,
		IEnumerable<Transaction> transactions,
		CancellationToken cancellationToken)
	{
		var errorOrAssets = await this.GetAssets(userId, transactions, cancellationToken);

		if (errorOrAssets.IsError)
		{
			return errorOrAssets.Errors;
		}

		return await this.CalculateAsync(
			currency,
			transactions,
			errorOrAssets.Value,
			this.CalculateTransactionResults,
			cancellationToken);
	}

	private async Task<ErrorOr<T>> CalculateAsync<T>(
		Currency currency,
		IEnumerable<Transaction> transactions,
		IEnumerable<Asset> assets,
		Func<IReadOnlyDictionary<AssetId, Asset>, IReadOnlyDictionary<InstrumentId, InvestmentInstrument>, IReadOnlyDictionary<InstrumentId, IReadOnlyDictionary<DateOnly, decimal>>, IReadOnlyDictionary<Currency, IReadOnlyDictionary<DateOnly, decimal>>, IEnumerable<Transaction>, T> calculator,
		CancellationToken cancellationToken)
	{
		var assetMap = assets.ToFrozenDictionary(x => x.Id, x => x);

		var errorOrInstrumentMap = await this.GetInvestmentInstrumentMap(assetMap.Values, cancellationToken);

		if (errorOrInstrumentMap.IsError)
		{
			return errorOrInstrumentMap.Errors;
		}

		var instrumentMap = errorOrInstrumentMap.Value;

		var errorOrHistoricalPricesMap = await this.GetHistoricalPricesMap(instrumentMap.Values, cancellationToken);

		if (errorOrHistoricalPricesMap.IsError)
		{
			return errorOrHistoricalPricesMap.Errors;
		}

		var errorOrHistoricalExchangeRatesMap = await this.GetHistoricalExchangeRatesMap(
			instrumentMap.Values.Select(x => x.Currency),
			currency,
			cancellationToken);

		if (errorOrHistoricalExchangeRatesMap.IsError)
		{
			return errorOrHistoricalExchangeRatesMap.Errors;
		}

		return calculator(assetMap, instrumentMap, errorOrHistoricalPricesMap.Value, errorOrHistoricalExchangeRatesMap.Value, transactions);
	}

	private async Task<ErrorOr<IEnumerable<Asset>>> GetAssets(
		UserId userId,
		IEnumerable<Transaction> transactions,
		CancellationToken cancellationToken)
	{
		var assets = new List<Asset>();

		foreach (var assetId in transactions.Select(x => x.AssetId).Distinct())
		{
			var errorOrAsset = await this.assetRepository.GetByIdAsync(userId, assetId, cancellationToken);

			if (errorOrAsset.IsError)
			{
				return errorOrAsset.Errors;
			}

			assets.Add(errorOrAsset.Value);
		}

		return assets;
	}

	private async Task<ErrorOr<IReadOnlyDictionary<InstrumentId, InvestmentInstrument>>> GetInvestmentInstrumentMap(
		IEnumerable<Asset> assets,
		CancellationToken cancellationToken)
	{
		var errorOrInstruments = await this.instrumentRepository.GetAllAsync(cancellationToken);

		if (errorOrInstruments.IsError)
		{
			return errorOrInstruments.Errors;
		}

		return errorOrInstruments.Value
			.Where(x => assets.Any(asset => asset.InstrumentId == x.Id))
			.ToFrozenDictionary(x => x.Id, x => x);
	}

	private async Task<ErrorOr<IReadOnlyDictionary<InstrumentId, IReadOnlyDictionary<DateOnly, decimal>>>> GetHistoricalPricesMap(
		IEnumerable<InvestmentInstrument> investmentInstruments,
		CancellationToken cancellationToken)
	{
		var historicalPrices = new Dictionary<InstrumentId, IReadOnlyDictionary<DateOnly, decimal>>();

		foreach (var investmentInstrument in investmentInstruments)
		{
			var errorOrHistoricalPrices = await this.GetHistoricalPrices(investmentInstrument, cancellationToken);

			if (errorOrHistoricalPrices.IsError)
			{
				return errorOrHistoricalPrices.Errors;
			}

			historicalPrices.Add(investmentInstrument.Id, errorOrHistoricalPrices.Value);
		}

		return historicalPrices.ToFrozenDictionary();
	}

	private async Task<ErrorOr<IReadOnlyDictionary<Currency, IReadOnlyDictionary<DateOnly, decimal>>>> GetHistoricalExchangeRatesMap(
		IEnumerable<Currency> fromCurrencies,
		Currency toCurrency,
		CancellationToken cancellationToken)
	{
		var historicalExchangeRates = new Dictionary<Currency, IReadOnlyDictionary<DateOnly, decimal>>();

		foreach (var fromCurrency in fromCurrencies.Distinct())
		{
			var errorOrHistoricalExchangeRates
				= await this.exchangeRateProvider.GetExchangeRatesAsync(fromCurrency, toCurrency, cancellationToken);

			if (errorOrHistoricalExchangeRates.IsError)
			{
				return errorOrHistoricalExchangeRates.Errors;
			}

			historicalExchangeRates.Add(fromCurrency, errorOrHistoricalExchangeRates.Value);
		}

		return historicalExchangeRates.ToFrozenDictionary();
	}

	private ValuationResult CalculateValuation(
		DateOnly evaluationDate,
		IReadOnlyDictionary<AssetId, Asset> assetMap,
		IReadOnlyDictionary<InstrumentId, InvestmentInstrument> instrumentMap,
		IReadOnlyDictionary<InstrumentId, IReadOnlyDictionary<DateOnly, decimal>> historicalPricesMap,
		IReadOnlyDictionary<Currency, IReadOnlyDictionary<DateOnly, decimal>> historicalExchangeRatesMap,
		IEnumerable<Transaction> transactions)
	{
		var valuationTransactions = transactions
			.Select(transaction =>
			{
				var asset = assetMap[transaction.AssetId];
				var instrument = instrumentMap[asset.InstrumentId];

				var historicalPrices = historicalPricesMap[instrument.Id];
				var historicalExchangeRates = historicalExchangeRatesMap[instrument.Currency];

				return this.CalculateValuationTransaction(historicalPrices, historicalExchangeRates, evaluationDate, transaction);
			})
			.ToImmutableArray();

		return new ValuationResult(
			valuationTransactions.Sum(x => x.InvestedValue),
			valuationTransactions.Sum(x => x.CurrentValue),
			100 * valuationTransactions.Select(x => ((evaluationDate.DayNumber - x.Date.DayNumber) / 365.25M, x.XIRRTransactionAmount, x.XIRRBalanceAmount)).CalculateXIRR());
	}

	private IEnumerable<TransactionResult> CalculateTransactionResults(
		IReadOnlyDictionary<AssetId, Asset> assetMap,
		IReadOnlyDictionary<InstrumentId, InvestmentInstrument> instrumentMap,
		IReadOnlyDictionary<InstrumentId, IReadOnlyDictionary<DateOnly, decimal>> historicalPricesMap,
		IReadOnlyDictionary<Currency, IReadOnlyDictionary<DateOnly, decimal>> historicalExchangeRatesMap,
		IEnumerable<Transaction> transactions)
	{
		return transactions.Select(transaction =>
		{
			var asset = assetMap[transaction.AssetId];
			var instrument = instrumentMap[asset.InstrumentId];

			return new TransactionResult(
				transaction.Id,
				transaction.Date,
				transaction.Name,
				transaction.Type,
				transaction.Units,
				transaction.CalculateTransactionAmount(historicalPricesMap[asset.InstrumentId], historicalExchangeRatesMap[instrument.Currency], transaction.Date));
		});
	}

	private ValuationTransaction CalculateValuationTransaction(
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates,
		DateOnly evaluationDate,
		Transaction transaction)
	{
		return new ValuationTransaction
		{
			Date = transaction.Date,
			InvestedValue = transaction.CalculateInitialBalanceAmount(historicalPrices, historicalExchangeRates, evaluationDate),
			CurrentValue = transaction.CalculateCurrentBalanceAmount(historicalPrices, historicalExchangeRates, evaluationDate),
			XIRRTransactionAmount = transaction.CalculateXIRRTransactionAmount(historicalPrices, historicalExchangeRates, evaluationDate),
			XIRRBalanceAmount = transaction.CalculateXIRRBalanceAmount(historicalPrices, historicalExchangeRates, evaluationDate),
		};
	}

	private async Task<ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>> GetHistoricalPrices(
		InvestmentInstrument investmentInstrument,
		CancellationToken cancellationToken)
	{
		return investmentInstrument switch
		{
			MutualFund mutualFund => await this.mutualFundApiClient.GetPriceAsync(mutualFund.SchemeCode, cancellationToken),
			Stock stock => await this.stockApiClient.GetPriceAsync(stock.Symbol, cancellationToken),
			_ => ImmutableDictionary<DateOnly, decimal>.Empty,
		};
	}

	private IEnumerable<DateOnly> GetEvaluationDates(
		IEnumerable<Transaction> transactions)
	{
		if (!transactions.Any())
		{
			return Enumerable.Empty<DateOnly>();
		}

		DateOnly startDate = transactions.Min(x => x.Date);
		DateOnly endDate = DateOnly.FromDateTime(DateTime.UtcNow);

		var evaluationDates = new List<DateOnly>();
		for (DateOnly date = startDate; date < endDate; date = date.AddMonths(1))
		{
			date = new DateOnly(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
			if (date >= endDate)
			{
				break;
			}

			evaluationDates.Add(date);
		}

		evaluationDates.Add(endDate);

		return evaluationDates.Distinct().ToImmutableArray();
	}

	private sealed class ValuationTransaction
	{
		public DateOnly Date { get; init; }

		public decimal InvestedValue { get; init; }

		public decimal CurrentValue { get; init; }

		public decimal XIRRTransactionAmount { get; init; }

		public decimal XIRRBalanceAmount { get; init; }
	}
}
