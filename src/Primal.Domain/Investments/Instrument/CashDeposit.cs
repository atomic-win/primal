using Primal.Domain.Money;

namespace Primal.Domain.Investments;

public sealed class CashDeposit : InvestmentInstrument
{
	public CashDeposit(InstrumentId id, InstrumentType type, Currency currency)
		: base(id, GetInstrumentName(type), type, currency)
	{
	}

	private static string GetInstrumentName(InstrumentType type)
	{
		return type switch
		{
			InstrumentType.CashAccounts => "Cash Account",
			InstrumentType.FixedDeposits => "Fixed Deposit",
			InstrumentType.EPF => "EPF",
			InstrumentType.PPF => "PPF",
			_ => "Unknown",
		};
	}
}
