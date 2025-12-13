using Primal.Domain.Common.Models;

namespace Primal.Domain.Investments;

public sealed class Transaction : Entity<TransactionId>
{
	public Transaction(
		TransactionId id,
		DateOnly date,
		string name,
		TransactionType transactionType,
		AssetItemId assetItemId,
		decimal units)
		: base(id)
	{
		this.Date = date;
		this.Name = name;
		this.TransactionType = transactionType;
		this.AssetItemId = assetItemId;
		this.Units = units;
	}

	public static Transaction Empty { get; } = new Transaction(
		TransactionId.Empty,
		DateOnly.MinValue,
		string.Empty,
		TransactionType.Buy,
		AssetItemId.Empty,
		0m);

	public DateOnly Date { get; init; }

	public string Name { get; init; }

	public TransactionType TransactionType { get; init; }

	public AssetItemId AssetItemId { get; init; }

	public decimal Units { get; init; }
}
