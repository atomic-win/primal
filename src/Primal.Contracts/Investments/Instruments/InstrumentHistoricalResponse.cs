namespace Primal.Contracts.Investments;

public sealed record InstrumentHistoricalResponse(
	Guid InstrumentId,
	IReadOnlyDictionary<DateOnly, decimal> Values);
