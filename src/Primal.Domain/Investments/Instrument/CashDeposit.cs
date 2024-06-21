using Primal.Domain.Money;

namespace Primal.Domain.Investments;

public sealed class CashDeposit : InvestmentInstrument
{
	public CashDeposit(InstrumentId id, Currency currency)
		: base(id, "Cash Deposit", InstrumentType.CashDeposits, currency)
	{
	}
}
