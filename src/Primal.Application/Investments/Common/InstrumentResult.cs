using Primal.Domain.Investments;

namespace Primal.Application.Investments;

public sealed record InstrumentResult(InstrumentId Id, string Name, InvestmentCategory Category, InvestmentType Type, AccountId AccountId);
