namespace Primal.Contracts.Investments;

public sealed record TransactionRequest(
	DateOnly Date,
	string Name,
	string Type,
	Guid AssetId,
	decimal Units);
