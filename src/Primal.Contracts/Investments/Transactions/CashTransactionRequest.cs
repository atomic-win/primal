namespace Primal.Contracts.Investments;

public sealed record CashTransactionRequest(
	DateOnly Date,
	string Name,
	string Type,
	Guid AssetId,
	decimal Amount,
	string Currency) : TransactionRequest(Date, Name, Type, AssetId);
