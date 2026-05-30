namespace Primal.Api.AssetItems;

internal sealed record ValuationResponse(
	DateOnly Date,
	decimal InvestedValue,
	decimal CurrentValue,
	decimal XirrPercent);
