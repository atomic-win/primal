using Primal.Domain.Money;

namespace Primal.Domain.Investments;

public sealed class CashTransaction : Transaction
{
	public CashTransaction(
		TransactionId id,
		DateOnly date,
		string name,
		TransactionType type,
		AssetId assetId,
		decimal amount,
		Currency currency)
		: base(id, date, name, type, assetId)
	{
		this.Amount = amount;
		this.Currency = currency;
	}

	public decimal Amount { get; init; }

	public Currency Currency { get; init; }
}
