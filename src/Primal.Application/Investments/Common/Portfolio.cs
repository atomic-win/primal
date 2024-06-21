namespace Primal.Application.Investments;

public sealed record Portfolio<T>(
	T Id,
	decimal InitialAmount,
	decimal InitialAmountPercent,
	decimal CurrentAmount,
	decimal CurrentAmountPercent,
	decimal XirrPercent);
