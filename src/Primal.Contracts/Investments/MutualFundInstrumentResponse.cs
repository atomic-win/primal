namespace Primal.Contracts.Investments;

public sealed record MutualFundInstrumentResponse(
	Guid Id,
	string Name,
	string Category,
	string Type,
	Guid MutualFundId) : InstrumentResponse(Id, Name, Category, Type);
