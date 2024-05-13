using ErrorOr;
using MediatR;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record GetAllInstrumentsQuery(UserId UserId) : IRequest<ErrorOr<IEnumerable<InstrumentResult>>>;
