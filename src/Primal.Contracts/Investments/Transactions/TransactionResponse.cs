namespace Primal.Contracts.Investments;

public sealed record TransactionResponse(
	Guid Id,
	DateOnly Date,
	string Name,
	string Type,
	Guid AssetId,
	decimal Units);
