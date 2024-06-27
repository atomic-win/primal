using ErrorOr;
using MediatR;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

public sealed record GetInstrumentPriceQuery(
	InstrumentId InstrumentId) : IRequest<ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>>;
