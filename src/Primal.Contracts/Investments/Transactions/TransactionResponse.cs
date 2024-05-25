namespace Primal.Contracts.Investments;

public abstract record TransactionResponse(
	Guid Id,
	DateOnly Date,
	string Name,
	string Type,
	Guid AssetId);
