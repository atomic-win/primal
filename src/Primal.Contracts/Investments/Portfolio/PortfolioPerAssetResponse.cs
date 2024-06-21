namespace Primal.Contracts.Investments;

public sealed record PortfolioPerAssetResponse(
	Guid AssetId,
	decimal InitialAmount,
	decimal InitialAmountPercent,
	decimal CurrentAmount,
	decimal CurrentAmountPercent,
	decimal XirrPercent);
