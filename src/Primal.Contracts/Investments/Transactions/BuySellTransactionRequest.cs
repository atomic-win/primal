namespace Primal.Contracts.Investments;

public sealed record BuySellTransactionRequest(
	DateOnly Date,
	string Name,
	string Type,
	Guid AssetId,
	decimal Units) : TransactionRequest(Date, Name, Type, AssetId);
