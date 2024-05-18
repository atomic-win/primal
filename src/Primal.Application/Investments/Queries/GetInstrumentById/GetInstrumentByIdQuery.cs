using ErrorOr;
using MediatR;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

public sealed record GetInstrumentByIdQuery(InstrumentId InstrumentId) : IRequest<ErrorOr<InvestmentInstrument>>;
