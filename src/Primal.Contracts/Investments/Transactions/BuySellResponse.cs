namespace Primal.Contracts.Investments;

public sealed record BuySellResponse(
	Guid Id,
	DateOnly Date,
	string Name,
	string Type,
	Guid AssetId,
	decimal Units) : TransactionResponse(Id, Date, Name, Type, AssetId);
