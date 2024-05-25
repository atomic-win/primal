namespace Primal.Contracts.Investments;

public sealed record CashDepositResponse(
	Guid Id,
	string Name,
	string Type,
	string Currency) : InstrumentResponse(Id, Name, Type, Currency);
