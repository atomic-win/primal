using ErrorOr;
using MediatR;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

public sealed record GetInstrumentValueQuery(
	InstrumentId InstrumentId,
	DateOnly StartDate,
	DateOnly EndDate) : IRequest<ErrorOr<IEnumerable<InstrumentValue>>>;
