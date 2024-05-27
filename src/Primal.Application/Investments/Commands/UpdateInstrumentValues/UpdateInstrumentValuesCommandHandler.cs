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
		var errorOrInstruments = await this.GetSortedInstrumentsAsync(cancellationToken);

		if (errorOrInstruments.IsError)
		{
			return errorOrInstruments.Errors;
		}

		List<Error> errors = new();
		foreach (var (date, instrument) in errorOrInstruments.Value)
		{
			var errorOrSuccess = await this.UpdateInstrumentValueAsync(instrument, date, cancellationToken);

			if (errorOrSuccess.IsError)
			{
				errors.AddRange(errorOrSuccess.Errors);
			}
		}

		return errors.Count > 0 ? errors : Result.Success;
	}

	private async Task<ErrorOr<SortedList<DateOnly, InvestmentInstrument>>> GetSortedInstrumentsAsync(CancellationToken cancellationToken)
	{
		var errorOrInstruments = await this.instrumentRepository.GetAllAsync(cancellationToken);

		if (errorOrInstruments.IsError)
		{
			return errorOrInstruments.Errors;
		}

		var cutOffDate = this.GetCutOffDate();
		var result = new SortedList<DateOnly, InvestmentInstrument>();

		foreach (var instrument in errorOrInstruments.Value)
		{
			if (instrument.Type != InstrumentType.MutualFunds
				&& instrument.Type != InstrumentType.Stocks)
			{
				continue;
			}

			var latestValueDate = await this.instrumentRepository.GetLatestInstrumentValueDateAsync(instrument.Id, cancellationToken);
			if (latestValueDate.IsError || latestValueDate.Value >= cutOffDate)
			{
				continue;
			}

			result.Add(latestValueDate.Value, instrument);
		}

		return result;
	}

	private async Task<ErrorOr<Success>> UpdateInstrumentValueAsync(InvestmentInstrument investmentInstrument, DateOnly startDate, CancellationToken cancellationToken)
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

		var errors = new List<Error>();
		foreach (var instrumentValue in errorOrInstrumentValues.Value.Where(x => x.Date >= startDate).OrderBy(x => x.Date))
		{
			var errorOrSuccess = await this.instrumentRepository.UpdateInstrumentValueAsync(investmentInstrument.Id, instrumentValue.Date, instrumentValue.Value, cancellationToken);
			if (errorOrSuccess.IsError)
			{
				errors.AddRange(errorOrSuccess.Errors);
			}
		}

		return errors.Count > 0 ? errors : Result.Success;
	}

	private DateOnly GetCutOffDate()
	{
		var date = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);

		while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
		{
			date = date.AddDays(-1);
		}

		return date;
	}
}
