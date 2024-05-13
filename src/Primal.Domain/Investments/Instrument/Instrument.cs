namespace Primal.Domain.Investments;

public sealed class Instrument
{
	public InstrumentId Id { get; set; }

	public string Name { get; set; }

	public InvestmentCategory Category { get; set; }

	public InvestmentType Type { get; set; }

	public AccountId AccountId { get; set; }
}
