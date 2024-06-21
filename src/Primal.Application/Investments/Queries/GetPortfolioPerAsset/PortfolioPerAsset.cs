using Primal.Domain.Investments;

namespace Primal.Application.Investments;

public sealed record PortfolioPerAsset(
	AssetId AssetId,
	decimal InitialAmount,
	decimal InitialAmountPercent,
	decimal CurrentAmount,
	decimal CurrentAmountPercent,
	decimal XirrPercent);
