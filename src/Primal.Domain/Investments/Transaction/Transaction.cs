using Primal.Domain.Common.Models;

namespace Primal.Domain.Investments;

public sealed class Transaction : Entity<TransactionId>
{
	public Transaction(
		TransactionId id,
		DateOnly date,
		string name,
		TransactionType type,
		AssetId assetId,
		decimal units)
		: base(id)
	{
		this.Date = date;
		this.Name = name;
		this.Type = type;
		this.AssetId = assetId;
		this.Units = units;
	}

	public DateOnly Date { get; init; }

	public string Name { get; init; }

	public TransactionType Type { get; init; }

	public AssetId AssetId { get; init; }

	public decimal Units { get; init; }
}
