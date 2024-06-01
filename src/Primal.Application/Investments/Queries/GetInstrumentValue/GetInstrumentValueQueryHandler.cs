using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class GetInstrumentValueQueryHandler : IRequestHandler<GetInstrumentValueQuery, ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>>
{
	private readonly IMutualFundApiClient mutualFundApiClient;
	private readonly IStockApiClient stockApiClient;
	private readonly IInstrumentRepository instrumentRepository;

	public GetInstrumentValueQueryHandler(
		IMutualFundApiClient mutualFundApiClient,
		IStockApiClient stockApiClient,
		IInstrumentRepository instrumentRepository)
	{
		this.mutualFundApiClient = mutualFundApiClient;
		this.stockApiClient = stockApiClient;
		this.instrumentRepository = instrumentRepository;
	}

	public async Task<ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>> Handle(GetInstrumentValueQuery request, CancellationToken cancellationToken)
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

		var errorOrInstrumentValues = instrument switch
		{
			MutualFund mutualFund => await this.mutualFundApiClient.GetHistoricalValuesAsync(mutualFund.SchemeCode, cancellationToken),
			Stock stock => await this.stockApiClient.GetHistoricalValuesAsync(stock.Symbol, cancellationToken),
			_ => Error.Validation(description: "Only mutual funds and stocks have historical values"),
		};

		if (errorOrInstrumentValues.IsError)
		{
			return errorOrInstrumentValues.Errors;
		}

		return errorOrInstrumentValues.Value
			.Where(kvp => kvp.Key >= request.StartDate && kvp.Key <= request.EndDate)
			.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
	}
}
