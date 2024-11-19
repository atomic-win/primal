using ErrorOr;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

internal sealed class TransactionResultCalculator
{
	private readonly IInstrumentRepository instrumentRepository;

	private readonly InstrumentPriceProvider instrumentPriceProvider;
	private readonly IExchangeRateProvider exchangeRateProvider;

	public TransactionResultCalculator(
		IInstrumentRepository instrumentRepository,
		InstrumentPriceProvider instrumentPriceProvider,
		IExchangeRateProvider exchangeRateProvider)
	{
		this.instrumentRepository = instrumentRepository;
		this.instrumentPriceProvider = instrumentPriceProvider;
		this.exchangeRateProvider = exchangeRateProvider;
	}

	internal async Task<ErrorOr<IEnumerable<TransactionResult>>> CalculateAsync(
		Asset asset,
		Currency currency,
		IEnumerable<Transaction> transactions,
		CancellationToken cancellationToken)
	{
		var errorOrInstrument = await this.instrumentRepository.GetByIdAsync(
			asset.InstrumentId,
			cancellationToken);

		if (errorOrInstrument.IsError)
		{
			return errorOrInstrument.Errors;
		}

		var errorOrHistoricalPrices = await this.instrumentPriceProvider.GetHistoricalPricesAsync(
			errorOrInstrument.Value,
			cancellationToken);

		if (errorOrHistoricalPrices.IsError)
		{
			return errorOrHistoricalPrices.Errors;
		}

		var errorOrExchangeRate = await this.exchangeRateProvider.GetExchangeRatesAsync(
			from: errorOrInstrument.Value.Currency,
			to: currency,
			cancellationToken);

		if (errorOrExchangeRate.IsError)
		{
			return errorOrExchangeRate.Errors;
		}

		return transactions.Select(transaction =>
			new TransactionResult(
					transaction.Id,
					transaction.Date,
					transaction.Name,
					transaction.Type,
					transaction.Units,
					transaction.CalculateTransactionAmount(
						errorOrHistoricalPrices.Value,
						errorOrExchangeRate.Value)))
			.ToErrorOr();
	}
}
