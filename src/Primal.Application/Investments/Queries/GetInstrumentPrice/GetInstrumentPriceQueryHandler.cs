using System.Collections.Immutable;
using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class GetInstrumentPriceQueryHandler : IRequestHandler<GetInstrumentPriceQuery, ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>>
{
	private readonly IMutualFundApiClient mutualFundApiClient;
	private readonly IStockApiClient stockApiClient;
	private readonly IInstrumentRepository instrumentRepository;

	public GetInstrumentPriceQueryHandler(
		IMutualFundApiClient mutualFundApiClient,
		IStockApiClient stockApiClient,
		IInstrumentRepository instrumentRepository)
	{
		this.mutualFundApiClient = mutualFundApiClient;
		this.stockApiClient = stockApiClient;
		this.instrumentRepository = instrumentRepository;
	}

	public async Task<ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>> Handle(GetInstrumentPriceQuery request, CancellationToken cancellationToken)
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
			return ImmutableDictionary<DateOnly, decimal>.Empty;
		}

		return instrument switch
		{
			MutualFund mutualFund => await this.mutualFundApiClient.GetPriceAsync(mutualFund.SchemeCode, cancellationToken),
			Stock stock => await this.stockApiClient.GetPriceAsync(stock.Symbol, cancellationToken),
			_ => ImmutableDictionary<DateOnly, decimal>.Empty,
		};
	}
}
