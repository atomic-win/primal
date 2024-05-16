using Primal.Domain.Investments;

namespace Primal.Application.Investments;

public abstract record InstrumentResult(InstrumentId Id, string Name, InvestmentCategory Category, InvestmentType Type);
