namespace Primal.Contracts.Investments;

public sealed record PortfolioResponse<T>(
	T Id,
	string Type,
	DateOnly Date,
	decimal InitialAmount,
	decimal InitialAmountPercent,
	decimal CurrentAmount,
	decimal CurrentAmountPercent,
	decimal XirrPercent) : PortfolioResponse(Type, Date, InitialAmount, InitialAmountPercent, CurrentAmount, CurrentAmountPercent, XirrPercent);

public abstract record PortfolioResponse(
	string Type,
	DateOnly Date,
	decimal InitialAmount,
	decimal InitialAmountPercent,
	decimal CurrentAmount,
	decimal CurrentAmountPercent,
	decimal XirrPercent);
