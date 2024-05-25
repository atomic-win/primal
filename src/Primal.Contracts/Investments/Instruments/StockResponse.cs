namespace Primal.Contracts.Investments;

public sealed record StockResponse(
	Guid Id,
	string Type,
	string Symbol,
	string Name,
	string StockType,
	string Region,
	string Currency) : InstrumentResponse(Id, Name, Type, Currency);
