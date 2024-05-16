using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record AddMutualFundInstrumentCommand(UserId UserId, string Name, InvestmentCategory Category, int SchemeCode) : IRequest<ErrorOr<MutualFundInstrumentResult>>;
