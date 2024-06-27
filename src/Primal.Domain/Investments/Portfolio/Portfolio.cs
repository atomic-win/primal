namespace Primal.Domain.Investments;

public sealed record Portfolio<T>(
	T Id,
	PortfolioType Type,
	decimal InitialAmount,
	decimal InitialAmountPercent,
	decimal CurrentAmount,
	decimal CurrentAmountPercent,
	decimal XirrPercent) : Portfolio(Type, InitialAmount, InitialAmountPercent, CurrentAmount, CurrentAmountPercent, XirrPercent);

public abstract record Portfolio(
	PortfolioType Type,
	decimal InitialAmount,
	decimal InitialAmountPercent,
	decimal CurrentAmount,
	decimal CurrentAmountPercent,
	decimal XirrPercent);
