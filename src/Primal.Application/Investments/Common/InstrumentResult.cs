using Primal.Domain.Investments;

namespace Primal.Application.Investments;

public record InstrumentResult(InstrumentId Id, string Name, InvestmentCategory Category, InvestmentType Type);
