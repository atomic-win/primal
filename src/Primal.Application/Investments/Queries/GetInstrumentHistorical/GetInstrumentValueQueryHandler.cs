using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class GetInstrumentValueQueryHandler : IRequestHandler<GetInstrumentValueQuery, ErrorOr<IEnumerable<InstrumentValue>>>
{
	private readonly IInstrumentRepository instrumentRepository;

	public GetInstrumentValueQueryHandler(IInstrumentRepository instrumentRepository)
	{
		this.instrumentRepository = instrumentRepository;
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

		return await this.instrumentRepository.GetInstrumentValueAsync(investmentInstrument.Id, date, cancellationToken);
	}
}
