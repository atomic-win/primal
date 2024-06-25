namespace Primal.Contracts.Investments;

public sealed record CashInstrumentResponse(
	Guid Id,
	string Name,
	string Type,
	string Currency) : InstrumentResponse(Id, Name, Type, Currency);
