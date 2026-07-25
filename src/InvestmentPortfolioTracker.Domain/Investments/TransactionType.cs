namespace InvestmentPortfolioTracker.Domain.Investments;

public enum TransactionType
{
	Unknown = 0,
	Buy,
	Sell,
	Deposit,
	Withdrawal,
	Dividend,
	Interest,
	SelfInterest,
	InterestPenalty,
}
