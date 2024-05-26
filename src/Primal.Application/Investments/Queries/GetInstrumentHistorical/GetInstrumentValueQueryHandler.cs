using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class GetInstrumentValueQueryHandler : IRequestHandler<GetInstrumentValueQuery, ErrorOr<IEnumerable<InstrumentValue>>>
{
	private readonly IInstrumentRepository instrumentRepository;
	private readonly IMutualFundApiClient mutualFundApiClient;
	private readonly IStockApiClient stockApiClient;

	public GetInstrumentValueQueryHandler(
		IInstrumentRepository instrumentRepository,
		IMutualFundApiClient mutualFundApiClient,
		IStockApiClient stockApiClient)
	{
		this.instrumentRepository = instrumentRepository;
		this.mutualFundApiClient = mutualFundApiClient;
		this.stockApiClient = stockApiClient;
	}

	public async Task<ErrorOr<IEnumerable<InstrumentValue>>> Handle(GetInstrumentValueQuery request, CancellationToken cancellationToken)
	{
		var errorOrInstrument = await this.instrumentRepository.GetByIdAsync(request.InstrumentId, cancellationToken);

		if (errorOrInstrument.IsError)
		{
			return errorOrInstrument.Errors;
		}

		var instrument = errorOrInstrument.Value;

		if (instrument.Type != InstrumentType.MutualFunds
			&& instrument.Type != InstrumentType.Stocks)
		{
			return Error.Validation(description: "Only mutual funds and stocks have historical values");
		}

		DateOnly startDate = request.StartDate;
		DateOnly endDate = request.EndDate;

		var result = new List<InstrumentValue>(endDate.DayNumber - startDate.DayNumber + 1);

		for (DateOnly date = startDate; date <= endDate; date = date.AddDays(1))
		{
			var errorOrValue = await this.GetInstrumentValueAsync(instrument, date, cancellationToken);

			if (errorOrValue.IsError)
			{
				return errorOrValue.Errors;
			}

			result.Add(new InstrumentValue(date, errorOrValue.Value));
		}

		return result;
	}

	private async Task<ErrorOr<decimal>> GetInstrumentValueAsync(InvestmentInstrument investmentInstrument, DateOnly date, CancellationToken cancellationToken)
	{
		while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
		{
			date = date.AddDays(-1);
		}

		var errorOrInstrumentValue = await this.instrumentRepository.GetInstrumentValueAsync(investmentInstrument.Id, date, cancellationToken);

		if (!errorOrInstrumentValue.IsError
			|| errorOrInstrumentValue.FirstError is not { Type: ErrorType.NotFound })
		{
			return errorOrInstrumentValue;
		}

		var errorOrLatestValueDate = await this.instrumentRepository.GetLatestInstrumentValueDateAsync(investmentInstrument.Id, cancellationToken);

		if (errorOrLatestValueDate.IsError)
		{
			return errorOrLatestValueDate.Errors;
		}

		if (date <= errorOrLatestValueDate.Value)
		{
			return await this.GetInstrumentValueAsync(investmentInstrument, date.AddDays(-1), cancellationToken);
		}

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

		foreach (var instrumentValue in errorOrInstrumentValues.Value.Where(x => x.Date > errorOrLatestValueDate.Value).OrderBy(x => x.Date))
		{
			await this.instrumentRepository.UpdateInstrumentValueAsync(investmentInstrument.Id, instrumentValue.Date, instrumentValue.Value, cancellationToken);
		}

		return await this.instrumentRepository.GetInstrumentValueAsync(investmentInstrument.Id, date, cancellationToken);
	}
}
