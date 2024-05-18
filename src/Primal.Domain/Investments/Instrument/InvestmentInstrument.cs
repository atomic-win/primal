using Primal.Domain.Common.Models;
using Primal.Domain.Money;

namespace Primal.Domain.Investments;

public abstract class InvestmentInstrument : Entity<InstrumentId>
{
	protected InvestmentInstrument(InstrumentId id, string name, InstrumentType type, Currency currency)
		: base(id)
	{
		this.Name = name;
		this.Type = type;
		this.Currency = currency;
	}

	public string Name { get; init; }

	public InstrumentType Type { get; init; }

	public Currency Currency { get; init; }
}
