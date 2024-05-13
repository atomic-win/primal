using ErrorOr;
using MediatR;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

public sealed record AddInstrumentCommand(string Name, InvestmentCategory Category, InvestmentType Type, AccountId AccountId) : IRequest<ErrorOr<InstrumentResult>>;
