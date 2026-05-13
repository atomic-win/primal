namespace Primal.Infrastructure.Persistence;

internal sealed class TransactionTableEntity : TableEntity
{
	public string Id { get; set; } = null!;

	public string Date { get; set; } = null!;

	public string Name { get; set; } = null!;

	public string TransactionType { get; set; } = null!;

	public string AssetItemId { get; set; } = null!;

	public string UserId { get; set; } = null!;

	public string Units { get; set; } = null!;

	public string Price { get; set; } = null!;

	public string Amount { get; set; } = null!;
}
