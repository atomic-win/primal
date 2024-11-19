using System.Collections.Immutable;
using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

internal sealed class GetValuationQueryHandler
	: IRequestHandler<GetValuationQuery, ErrorOr<ValuationResult>>
{
	private readonly ITransactionRepository transactionRepository;
	private readonly IAssetRepository assetRepository;
	private readonly IInstrumentRepository instrumentRepository;

	private readonly InstrumentPriceProvider instrumentPriceProvider;
	private readonly IExchangeRateProvider exchangeRateProvider;

	public GetValuationQueryHandler(
		ITransactionRepository transactionRepository,
		IAssetRepository assetRepository,
		IInstrumentRepository instrumentRepository,
		InstrumentPriceProvider instrumentPriceProvider,
		IExchangeRateProvider exchangeRateProvider)
	{
		this.transactionRepository = transactionRepository;
		this.assetRepository = assetRepository;
		this.instrumentRepository = instrumentRepository;
		this.instrumentPriceProvider = instrumentPriceProvider;
		this.exchangeRateProvider = exchangeRateProvider;
	}

	public async Task<ErrorOr<ValuationResult>> Handle(GetValuationQuery request, CancellationToken cancellationToken)
	{
		var errorOrAssets = await this.assetRepository.GetAllAsync(request.UserId, cancellationToken);

		if (errorOrAssets.IsError)
		{
			return errorOrAssets.Errors;
		}

		var assetIds = request.AssetIds;
		var assets = errorOrAssets.Value.Where(x => assetIds.Contains(x.Id)).ToImmutableArray();

		foreach (var assetId in assetIds)
		{
			if (!assets.Any(x => x.Id == assetId))
			{
				return Error.NotFound(description: $"Asset with ID '{assetId}' not found.");
			}
		}

		decimal investedValue = 0;
		decimal currentValue = 0;
		var xirrInputs = new List<(decimal Days, decimal Amount, decimal Balance)>();

		foreach (var asset in assets)
		{
			var errorOrAssetValuation = await this.CalculateAssetValuationAsync(
				request.UserId,
				request.Date,
				asset,
				request.Currency,
				cancellationToken);

			if (errorOrAssetValuation.IsError)
			{
				return errorOrAssetValuation.Errors;
			}

			var (assetInvestedValue, assetCurrentValue, assetXirrInputs) = errorOrAssetValuation.Value;

			investedValue += assetInvestedValue;
			currentValue += assetCurrentValue;
			xirrInputs.AddRange(assetXirrInputs);
		}

		return new ValuationResult(
			investedValue,
			currentValue,
			100 * xirrInputs.CalculateXIRR());
	}

	private async Task<ErrorOr<(decimal InvestedValue, decimal CurrentValue, IEnumerable<(decimal Days, decimal Amount, decimal Balance)> XirrInputs)>> CalculateAssetValuationAsync(
		UserId userId,
		DateOnly evaluationDate,
		Asset asset,
		Currency currency,
		CancellationToken cancellationToken)
	{
		var errorOrTransactions = await this.transactionRepository.GetByAssetIdAsync(
			userId,
			asset.Id,
			cancellationToken);

		if (errorOrTransactions.IsError)
		{
			return errorOrTransactions.Errors;
		}

		var errorOrInstrument = await this.instrumentRepository.GetByIdAsync(asset.InstrumentId, cancellationToken);

		if (errorOrInstrument.IsError)
		{
			return errorOrInstrument.Errors;
		}

		var instrument = errorOrInstrument.Value;

		var errorOrHistoricalPrices = await this.instrumentPriceProvider.GetHistoricalPricesAsync(
			instrument,
			cancellationToken);

		if (errorOrHistoricalPrices.IsError)
		{
			return errorOrHistoricalPrices.Errors;
		}

		var errorOrHistoricalExchangeRates = await this.exchangeRateProvider.GetExchangeRatesAsync(
			from: instrument.Currency,
			to: currency,
			cancellationToken);

		if (errorOrHistoricalExchangeRates.IsError)
		{
			return errorOrHistoricalExchangeRates.Errors;
		}

		var transactions = errorOrTransactions.Value.Where(transaction => transaction.Date <= evaluationDate).ToImmutableArray();
		var historicalPrices = errorOrHistoricalPrices.Value;
		var historicalExchangeRates = errorOrHistoricalExchangeRates.Value;

		var xirrInputs = transactions.Select(transaction => (
				(evaluationDate.DayNumber - transaction.Date.DayNumber) / 365.25m,
				transaction.CalculateXIRRTransactionAmount(historicalPrices, historicalExchangeRates, evaluationDate),
				transaction.CalculateXIRRBalanceAmount(historicalPrices, historicalExchangeRates, evaluationDate)));

		return (
			transactions.CalculateInvestedValue(historicalPrices, historicalExchangeRates, evaluationDate),
			transactions.CalculateCurrentValue(historicalPrices, historicalExchangeRates, evaluationDate),
			xirrInputs);
	}
}
