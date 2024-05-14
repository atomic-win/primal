using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record AddInstrumentCommand(UserId UserId, string Name, InvestmentCategory Category, InvestmentType Type) : IRequest<ErrorOr<InstrumentResult>>;
