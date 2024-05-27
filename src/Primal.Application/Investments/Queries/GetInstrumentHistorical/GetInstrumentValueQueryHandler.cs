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

		var errorOrInstrumentValues = await this.instrumentRepository.GetInstrumentValuesAsync(instrument.Id, cancellationToken);

		if (errorOrInstrumentValues.IsError)
		{
			return errorOrInstrumentValues.Errors;
		}

		var instrumentValuesMap = errorOrInstrumentValues.Value;
		DateOnly startDate = request.StartDate;
		DateOnly endDate = request.EndDate;

		var result = new List<InstrumentValue>(endDate.DayNumber - startDate.DayNumber + 1);

		for (DateOnly date = startDate; date <= endDate; date = date.AddDays(1))
		{
			if (!instrumentValuesMap.TryGetValue(date, out decimal value))
			{
				continue;
			}

			result.Add(new InstrumentValue(date, value));
		}

		return result;
	}
}
