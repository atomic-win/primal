using Primal.Domain.Investments;

namespace Primal.Application.Investments;

public sealed record MutualFundInstrumentResult(
	InstrumentId Id,
	string Name,
	InvestmentCategory Category,
	InvestmentType Type,
	MutualFundId MutualFundId) : InstrumentResult(Id, Name, Category, Type);
