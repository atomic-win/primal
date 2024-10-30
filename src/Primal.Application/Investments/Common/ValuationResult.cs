namespace Primal.Application.Investments;

public sealed record ValuationResult(
	decimal InvestedValue,
	decimal CurrentValue,
	decimal XirrPercent);
