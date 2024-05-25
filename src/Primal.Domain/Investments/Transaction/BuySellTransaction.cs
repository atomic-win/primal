namespace Primal.Domain.Investments;

public sealed class BuySellTransaction : Transaction
{
	public BuySellTransaction(TransactionId id, DateOnly date, string name, TransactionType type, AssetId assetId, decimal units)
		: base(id, date, name, type, assetId)
	{
		this.Units = units;
	}

	public decimal Units { get; init; }
}
