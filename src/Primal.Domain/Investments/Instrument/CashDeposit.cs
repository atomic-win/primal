using Primal.Domain.Money;

namespace Primal.Domain.Investments;

public sealed class CashDeposit : InvestmentInstrument
{
	public CashDeposit(InstrumentId id)
		: base(id, "Cash Deposit", InstrumentType.CashDeposits, Currency.Unknown)
	{
	}
}
