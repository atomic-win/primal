using System.Collections.Frozen;
using System.Collections.Immutable;
using ErrorOr;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

internal sealed class PortfolioCalculator
{
	private readonly ITransactionRepository transactionRepository;
	private readonly IAssetRepository assetRepository;
	private readonly IInstrumentRepository instrumentRepository;

	private readonly IMutualFundApiClient mutualFundApiClient;
	private readonly IStockApiClient stockApiClient;
	private readonly IExchangeRateProvider exchangeRateProvider;

	public PortfolioCalculator(
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

	internal async Task<ErrorOr<IEnumerable<Portfolio<T>>>> CalculateAsync<T>(
		UserId userId,
		Currency currency,
		Func<Transaction, Asset, InvestmentInstrument, T> idSelector,
		CancellationToken cancellationToken)
	{
		var errorOrTransactions = await this.transactionRepository.GetAllAsync(userId, cancellationToken);

		if (errorOrTransactions.IsError)
		{
			return errorOrTransactions.Errors;
		}

		var errorOrAssets = await this.assetRepository.GetAllAsync(userId, cancellationToken);

		if (errorOrAssets.IsError)
		{
			return errorOrAssets.Errors;
		}

		var transactions = errorOrTransactions.Value;
		var assets = errorOrAssets.Value;

		var errorOrInstrumentMap = await this.GetInvestmentInstrumentMap(assets, cancellationToken);

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

		return this.CalculatePortfolios<T>(
			idSelector,
			assets,
			instrumentMap,
			errorOrHistoricalPricesMap.Value,
			errorOrHistoricalExchangeRatesMap.Value,
			transactions).ToErrorOr();
	}

	private async Task<ErrorOr<IReadOnlyDictionary<InstrumentId, InvestmentInstrument>>> GetInvestmentInstrumentMap(
		IEnumerable<Asset> assets,
		CancellationToken cancellationToken)
	{
		var instruments = new List<InvestmentInstrument>();

		foreach (var instrumentId in assets.Select(x => x.InstrumentId).Distinct())
		{
			var errorOrInstrument = await this.instrumentRepository.GetByIdAsync(instrumentId, cancellationToken);

			if (errorOrInstrument.IsError)
			{
				return errorOrInstrument.Errors;
			}

			instruments.Add(errorOrInstrument.Value);
		}

		return instruments.ToFrozenDictionary(x => x.Id, x => x);
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

	private IEnumerable<Portfolio<T>> CalculatePortfolios<T>(
		Func<Transaction, Asset, InvestmentInstrument, T> idSelector,
		IEnumerable<Asset> assets,
		IReadOnlyDictionary<InstrumentId, InvestmentInstrument> instrumentMap,
		IReadOnlyDictionary<InstrumentId, IReadOnlyDictionary<DateOnly, decimal>> historicalPricesMap,
		IReadOnlyDictionary<Currency, IReadOnlyDictionary<DateOnly, decimal>> historicalExchangeRatesMap,
		IEnumerable<Transaction> transactions)
	{
		var today = DateOnly.FromDateTime(DateTime.UtcNow);
		var assetMap = assets.ToFrozenDictionary(x => x.Id, x => x);

		transactions = transactions.Concat(assets.Select(x => new Transaction(
			TransactionId.Empty,
			date: today,
			"Dummy transaction for asset",
			TransactionType.Unknown,
			x.Id,
			0.0M)));

		var idToPortfolioTransactions = new Dictionary<T, List<PortfolioTransaction>>();

		foreach (var transaction in transactions)
		{
			var asset = assetMap[transaction.AssetId];
			var instrument = instrumentMap[asset.InstrumentId];

			T id = idSelector(transaction, asset, instrument);

			if (!idToPortfolioTransactions.TryGetValue(id, out var portfolioTransactions))
			{
				portfolioTransactions = new List<PortfolioTransaction>();
				idToPortfolioTransactions.Add(id, portfolioTransactions);
			}

			portfolioTransactions.Add(this.CalculatePortfolioTransaction(
				historicalPricesMap[asset.InstrumentId],
				historicalExchangeRatesMap[instrument.Currency],
				transaction));
		}

		var portfolios = idToPortfolioTransactions
			.Select(kvp => new Portfolio<T>(
				kvp.Key,
				kvp.Value.Sum(x => x.InitialBalanceAmount),
				0.0M,
				kvp.Value.Sum(x => x.CurrentBalanceAmount),
				0.0M,
				100 * kvp.Value.Select(x => ((today.DayNumber - x.Date.DayNumber) / 365.25M, x.XIRRTransactionAmount, x.XIRRBalanceAmount)).CalculateXIRR()))
			.ToImmutableArray();

		decimal totalInitialAmount = portfolios.Sum(x => x.InitialAmount);
		decimal totalCurrentAmount = portfolios.Sum(x => x.CurrentAmount);

		return portfolios
			.Select(x => x with
			{
				InitialAmountPercent = 100 * x.InitialAmount / totalInitialAmount,
				CurrentAmountPercent = 100 * x.CurrentAmount / totalCurrentAmount,
			})
			.ToImmutableArray();
	}

	private Portfolio<T> CalculatePortfolio<T>(
		T id,
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates,
		IEnumerable<Transaction> transactions)
	{
		DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
		var portfolioTransactions = transactions
			.Select(transaction => this.CalculatePortfolioTransaction(historicalPrices, historicalExchangeRates, transaction))
			.ToImmutableArray();

		return new Portfolio<T>(
			id,
			portfolioTransactions.Sum(x => x.InitialBalanceAmount),
			0.0M,
			portfolioTransactions.Sum(x => x.CurrentBalanceAmount),
			0.0M,
			100 * portfolioTransactions.Select(x => ((today.DayNumber - x.Date.DayNumber) / 365.25M, x.XIRRTransactionAmount, x.XIRRBalanceAmount)).CalculateXIRR());
	}

	private PortfolioTransaction CalculatePortfolioTransaction(
		IReadOnlyDictionary<DateOnly, decimal> historicalPrices,
		IReadOnlyDictionary<DateOnly, decimal> historicalExchangeRates,
		Transaction transaction)
	{
		return new PortfolioTransaction
		{
			Date = transaction.Date,
			Type = transaction.Type,
			InitialBalanceAmount = transaction.CalculateInitialBalanceAmount(historicalPrices, historicalExchangeRates),
			CurrentBalanceAmount = transaction.CalculateCurrentBalanceAmount(historicalPrices, historicalExchangeRates),
			XIRRTransactionAmount = transaction.CalculateXIRRTransactionAmount(historicalPrices, historicalExchangeRates),
			XIRRBalanceAmount = transaction.CalculateXIRRBalanceAmount(historicalPrices, historicalExchangeRates),
		};
	}

	private async Task<ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>> GetHistoricalPrices(
		InvestmentInstrument investmentInstrument,
		CancellationToken cancellationToken)
	{
		return investmentInstrument switch
		{
			MutualFund mutualFund => await this.mutualFundApiClient.GetHistoricalValuesAsync(mutualFund.SchemeCode, cancellationToken),
			Stock stock => await this.stockApiClient.GetHistoricalValuesAsync(stock.Symbol, cancellationToken),
			_ => ImmutableDictionary<DateOnly, decimal>.Empty,
		};
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
