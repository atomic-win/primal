using Primal.Domain.Money;

namespace Primal.Domain.Investments;

public sealed class CashInstrument : InvestmentInstrument
{
	public CashInstrument(InstrumentId id, InstrumentType type, Currency currency)
		: base(id, GetInstrumentName(type, currency), type, currency)
	{
	}

	private static string GetInstrumentName(InstrumentType type, Currency currency)
	{
		return type switch
		{
			InstrumentType.CashAccounts => $"Cash Account - {currency}",
			InstrumentType.FixedDeposits => $"Fixed Deposit - {currency}",
			InstrumentType.EPF => $"EPF - {currency}",
			InstrumentType.PPF => $"PPF - {currency}",
			_ => "Unknown",
		};
	}
}
