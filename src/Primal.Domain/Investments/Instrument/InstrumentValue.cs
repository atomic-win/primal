namespace Primal.Domain.Investments;

public sealed record InstrumentValue(
	DateOnly Date,
	decimal Value);
