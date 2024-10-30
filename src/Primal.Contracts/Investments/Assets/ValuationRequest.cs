namespace Primal.Contracts.Investments;

public sealed record ValuationRequest(
	DateOnly Date,
	IReadOnlyCollection<Guid> AssetIds,
	string Currency);
