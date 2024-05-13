using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record GetInstrumentByIdQuery(UserId UserId, InstrumentId InstrumentId) : IRequest<ErrorOr<InstrumentResult>>;
