namespace Primal.Contracts.Investments;

public sealed record PortfolioRequest(
	IReadOnlyCollection<Guid> AssetIds,
	string Currency);
