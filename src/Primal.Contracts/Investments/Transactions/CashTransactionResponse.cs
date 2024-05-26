namespace Primal.Contracts.Investments;

public sealed record CashTransactionResponse(
	Guid Id,
	DateOnly Date,
	string Name,
	string Type,
	Guid AssetId,
	decimal Amount,
	string Currency) : TransactionResponse(Id, Date, Name, Type, AssetId);
