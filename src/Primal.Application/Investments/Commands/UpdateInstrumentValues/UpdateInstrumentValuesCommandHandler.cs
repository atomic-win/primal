using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class UpdateInstrumentValuesCommandHandler : IRequestHandler<UpdateInstrumentValuesCommand, ErrorOr<Success>>
{
	private readonly TimeProvider timeProvider;
	private readonly IInstrumentRepository instrumentRepository;
	private readonly IMutualFundApiClient mutualFundApiClient;
	private readonly IStockApiClient stockApiClient;

	public UpdateInstrumentValuesCommandHandler(
		TimeProvider timeProvider,
		IInstrumentRepository instrumentRepository,
		IMutualFundApiClient mutualFundApiClient,
		IStockApiClient stockApiClient)
	{
		this.timeProvider = timeProvider;
		this.instrumentRepository = instrumentRepository;
		this.mutualFundApiClient = mutualFundApiClient;
		this.stockApiClient = stockApiClient;
	}

	public async Task<ErrorOr<Success>> Handle(UpdateInstrumentValuesCommand request, CancellationToken cancellationToken)
	{
		var errorOrInstruments = await this.GetInstrumentsAsync(cancellationToken);

		if (errorOrInstruments.IsError)
		{
			return errorOrInstruments.Errors;
		}

		List<Error> errors = new();
		foreach (var instrument in errorOrInstruments.Value)
		{
			var errorOrSuccess = await this.UpdateInstrumentValueAsync(instrument, cancellationToken);

			if (errorOrSuccess.IsError)
			{
				errors.AddRange(errorOrSuccess.Errors);
			}
		}

		return errors.Count > 0 ? errors : Result.Success;
	}

	private async Task<ErrorOr<IEnumerable<InvestmentInstrument>>> GetInstrumentsAsync(CancellationToken cancellationToken)
	{
		var errorOrInstruments = await this.instrumentRepository.GetAllAsync(cancellationToken);

		if (errorOrInstruments.IsError)
		{
			return errorOrInstruments.Errors;
		}

		var latestValueDate = this.GetLatestValueDate();
		var result = new List<(DateOnly RefreshedDate, InvestmentInstrument Instrument)>();

		foreach (var instrument in errorOrInstruments.Value)
		{
			if (instrument.Type != InstrumentType.MutualFunds
				&& instrument.Type != InstrumentType.Stocks)
			{
				continue;
			}

			var errorOrRefreshedDate = await this.instrumentRepository.GetInstrumentValuesRefreshedDateAsync(instrument.Id, cancellationToken);
			if (errorOrRefreshedDate.IsError || errorOrRefreshedDate.Value >= latestValueDate)
			{
				continue;
			}

			result.Add((errorOrRefreshedDate.Value, instrument));
		}

		return result
			.OrderBy(x => x.RefreshedDate)
			.Select(x => x.Instrument)
			.ToArray();
	}

	private async Task<ErrorOr<Success>> UpdateInstrumentValueAsync(InvestmentInstrument investmentInstrument, CancellationToken cancellationToken)
	{
		var errorOrInstrumentValues = investmentInstrument switch
		{
			MutualFund mutualFund => await this.mutualFundApiClient.GetHistoricalValuesAsync(mutualFund.SchemeCode, cancellationToken),
			Stock stock => await this.stockApiClient.GetHistoricalValuesAsync(stock.Symbol, cancellationToken),
			_ => Error.Validation(description: "Only mutual funds and stocks have historical values"),
		};

		if (errorOrInstrumentValues.IsError)
		{
			return errorOrInstrumentValues.Errors;
		}

		return await this.instrumentRepository.UpdateInstrumentValuesAsync(investmentInstrument.Id, errorOrInstrumentValues.Value, cancellationToken);
	}

	private DateOnly GetLatestValueDate()
	{
		var date = DateOnly.FromDateTime(this.timeProvider.GetUtcNow().Date);

		while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
		{
			date = date.AddDays(-1);
		}

		return date;
	}
}
