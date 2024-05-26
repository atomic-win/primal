namespace Primal.Contracts.Investments;

public abstract record TransactionRequest(
	DateOnly Date,
	string Name,
	string Type,
	Guid AssetId);
