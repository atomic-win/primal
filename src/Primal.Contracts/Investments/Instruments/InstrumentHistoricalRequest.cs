namespace Primal.Contracts.Investments;

public sealed record InstrumentHistoricalRequest(
	DateOnly StartDate,
	DateOnly EndDate);
