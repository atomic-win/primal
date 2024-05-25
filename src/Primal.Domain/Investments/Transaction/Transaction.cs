using Primal.Domain.Common.Models;

namespace Primal.Domain.Investments;

public abstract class Transaction : Entity<TransactionId>
{
	protected Transaction(TransactionId id, DateOnly date, string name, TransactionType type, AssetId assetId)
		: base(id)
	{
		this.Date = date;
		this.Name = name;
		this.Type = type;
		this.AssetId = assetId;
	}

	public DateOnly Date { get; init; }

	public string Name { get; init; }

	public TransactionType Type { get; init; }

	public AssetId AssetId { get; init; }
}
