namespace Primal.Contracts.Investments;

public sealed record PortfolioResponse<T>(
	T Id,
	string Type,
	decimal InitialAmount,
	decimal InitialAmountPercent,
	decimal CurrentAmount,
	decimal CurrentAmountPercent,
	decimal XirrPercent) : PortfolioResponse(Type, InitialAmount, InitialAmountPercent, CurrentAmount, CurrentAmountPercent, XirrPercent);

public abstract record PortfolioResponse(
	string Type,
	decimal InitialAmount,
	decimal InitialAmountPercent,
	decimal CurrentAmount,
	decimal CurrentAmountPercent,
	decimal XirrPercent);
