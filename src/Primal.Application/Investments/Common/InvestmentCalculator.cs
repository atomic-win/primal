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

	internal async Task<ErrorOr<IEnumerable<Portfolio>>> CalculatePortfolioAsync(
		UserId userId,
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

			transactions.AddRange(errorOrTransactions.Value);
		}

		return await this.CalculateAsync(
			currency,
			transactions,
			assets,
			this.CalculatePortfolios,
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

	private IEnumerable<Portfolio> CalculatePortfolios(
		IReadOnlyDictionary<AssetId, Asset> assetMap,
		IReadOnlyDictionary<InstrumentId, InvestmentInstrument> instrumentMap,
		IReadOnlyDictionary<InstrumentId, IReadOnlyDictionary<DateOnly, decimal>> historicalPricesMap,
		IReadOnlyDictionary<Currency, IReadOnlyDictionary<DateOnly, decimal>> historicalExchangeRatesMap,
		IEnumerable<Transaction> transactions)
	{
		var today = DateOnly.FromDateTime(DateTime.UtcNow);
		transactions = transactions.Concat(assetMap.Keys.Select(assetId => new Transaction(
			TransactionId.Empty,
			date: today,
			"Dummy transaction for asset",
			TransactionType.Unknown,
			assetId,
			0.0M)))
			.ToImmutableArray();

		var portfolios = new List<Portfolio>();

		return this.GetEvaluationDates(transactions)
			.AsParallel()
			.AsUnordered()
			.SelectMany(evaluationDate =>
				this.CalculatePortfolios(
					evaluationDate,
					assetMap,
					instrumentMap,
					historicalPricesMap,
					historicalExchangeRatesMap,
					transactions)).ToImmutableArray();
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

	private IEnumerable<Portfolio> CalculatePortfolios(
		DateOnly evaluationDate,
		IReadOnlyDictionary<AssetId, Asset> assetMap,
		IReadOnlyDictionary<InstrumentId, InvestmentInstrument> instrumentMap,
		IReadOnlyDictionary<InstrumentId, IReadOnlyDictionary<DateOnly, decimal>> historicalPricesMap,
		IReadOnlyDictionary<Currency, IReadOnlyDictionary<DateOnly, decimal>> historicalExchangeRatesMap,
		IEnumerable<Transaction> transactions)
	{
		transactions = transactions.Where(x => x.Date <= evaluationDate).ToImmutableArray();

		IEnumerable<Portfolio> CalculatePortfolios<T>(
			PortfolioType portfolioType,
			Func<Asset, InvestmentInstrument, T> idSelector)
		{
			return this.CalculatePortfolios(
				evaluationDate,
				portfolioType,
				idSelector,
				assetMap,
				instrumentMap,
				historicalPricesMap,
				historicalExchangeRatesMap,
				transactions);
		}

		var portfoliosOverall = CalculatePortfolios(
			PortfolioType.Overall,
			(Asset asset, InvestmentInstrument instrument) => AssetId.Empty);

		var portfoliosPerInstrumentType = CalculatePortfolios(
			PortfolioType.PerInvestmentInstrumentType,
			(Asset asset, InvestmentInstrument instrument) => instrument.Type);

		var portfoliosPerInstrument = CalculatePortfolios(
			PortfolioType.PerInvestmentInstrument,
			(Asset asset, InvestmentInstrument instrument) => instrument.Id);

		var portfoliosPerAsset = CalculatePortfolios(
			PortfolioType.PerAsset,
			(Asset asset, InvestmentInstrument instrument) => asset.Id);

		return portfoliosOverall
			.Concat(portfoliosPerInstrumentType)
			.Concat(portfoliosPerInstrument)
			.Concat(portfoliosPerAsset);
	}

	private IEnumerable<Portfolio> CalculatePortfolios<T>(
		DateOnly evaluationDate,
		PortfolioType portfolioType,
		Func<Asset, InvestmentInstrument, T> idSelector,
		IReadOnlyDictionary<AssetId, Asset> assetMap,
		IReadOnlyDictionary<InstrumentId, InvestmentInstrument> instrumentMap,
		IReadOnlyDictionary<InstrumentId, IReadOnlyDictionary<DateOnly, decimal>> historicalPricesMap,
		IReadOnlyDictionary<Currency, IReadOnlyDictionary<DateOnly, decimal>> historicalExchangeRatesMap,
		IEnumerable<Transaction> transactions)
	{
		var idToPortfolioTransactions = new Dictionary<T, List<PortfolioTransaction>>();

		foreach (var transaction in transactions)
		{
			var asset = assetMap[transaction.AssetId];
			var instrument = instrumentMap[asset.InstrumentId];

			var historicalPrices = historicalPricesMap[instrument.Id];
			var historicalExchangeRates = historicalExchangeRatesMap[instrument.Currency];

			var portfolioTransaction = this.CalculatePortfolioTransaction(historicalPrices, historicalExchangeRates, evaluationDate, transaction);

			T id = idSelector(asset, instrument);

			if (!idToPortfolioTransactions.TryGetValue(id, out var portfolioTransactions))
			{
				portfolioTransactions = new List<PortfolioTransaction>();
				idToPortfolioTransactions.Add(id, portfolioTransactions);
			}

			portfolioTransactions.Add(portfolioTransaction);
		}

		var portfolios = idToPortfolioTransactions
			.AsParallel()
			.AsUnordered()
			.Select(kvp => new Portfolio<T>(
				kvp.Key,
				portfolioType,
				evaluationDate,
				kvp.Value.Sum(x => x.InitialBalanceAmount),
				0.0M,
				kvp.Value.Sum(x => x.CurrentBalanceAmount),
				0.0M,
				100 * kvp.Value.Select(x => ((evaluationDate.DayNumber - x.Date.DayNumber) / 365.25M, x.XIRRTransactionAmount, x.XIRRBalanceAmount)).CalculateXIRR()))
			.ToImmutableArray();

		decimal totalInitialAmount = portfolios.Sum(x => x.InitialAmount);
		decimal totalCurrentAmount = portfolios.Sum(x => x.CurrentAmount);

		return portfolios
			.Select(x => x with
			{
				InitialAmountPercent = 100 * x.InitialAmount / Math.Max(1.0M, totalInitialAmount),
				CurrentAmountPercent = 100 * x.CurrentAmount / Math.Max(1.0M, totalCurrentAmount),
			});
	}

	private PortfolioTransaction CalculatePortfolioTransaction(
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates,
		DateOnly evaluationDate,
		Transaction transaction)
	{
		return new PortfolioTransaction
		{
			Date = transaction.Date,
			Type = transaction.Type,
			InitialBalanceAmount = transaction.CalculateInitialBalanceAmount(historicalPrices, historicalExchangeRates, evaluationDate),
			CurrentBalanceAmount = transaction.CalculateCurrentBalanceAmount(historicalPrices, historicalExchangeRates, evaluationDate),
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

	private sealed class PortfolioTransaction
	{
		public DateOnly Date { get; init; }

		public TransactionType Type { get; init; }

		public decimal InitialBalanceAmount { get; init; }

		public decimal CurrentBalanceAmount { get; init; }

		public decimal XIRRTransactionAmount { get; init; }

		public decimal XIRRBalanceAmount { get; init; }
	}
}
