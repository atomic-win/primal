namespace Primal.Contracts.Investments;

public sealed record PortfolioResponse<T>(
	T Id,
	decimal InitialAmount,
	decimal InitialAmountPercent,
	decimal CurrentAmount,
	decimal CurrentAmountPercent,
	decimal XirrPercent);
