namespace Primal.Contracts.Investments;

public sealed record BuySellTransactionResponse(
	Guid Id,
	DateOnly Date,
	string Name,
	string Type,
	Guid AssetId,
	decimal Units) : TransactionResponse(Id, Date, Name, Type, AssetId);
