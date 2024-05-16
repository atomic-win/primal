using Primal.Domain.Common.Models;

namespace Primal.Domain.Investments;

public abstract class InvestmentInstrument : Entity<InstrumentId>
{
	protected InvestmentInstrument(InstrumentId id, string name, InvestmentCategory category, InvestmentType type)
		: base(id)
	{
		this.Name = name;
		this.Category = category;
		this.Type = type;
	}

	public string Name { get; init; }

	public InvestmentCategory Category { get; init; }

	public InvestmentType Type { get; init; }
}
