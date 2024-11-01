namespace Primal.Contracts.Investments;

public sealed record ValuationResponse(
	decimal InvestedValue,
	decimal CurrentValue,
	decimal XirrPercent);
