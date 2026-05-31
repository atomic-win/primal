using Primal.Domain.Investments;

namespace Primal.Domain.UnitTests.Investments;

public sealed class TransactionTests
{
	[Test]
	public async Task Empty_ReturnsExpectedDefaultValues()
	{
		var transaction = Transaction.Empty;

		await Assert.That(transaction.Id == TransactionId.Empty).IsTrue();
		await Assert.That(transaction.Date == DateOnly.MinValue).IsTrue();
		await Assert.That(string.Equals(transaction.Name, string.Empty, StringComparison.Ordinal)).IsTrue();
		await Assert.That(transaction.TransactionType == TransactionType.Buy).IsTrue();
		await Assert.That(transaction.AssetItemId == AssetItemId.Empty).IsTrue();
		await Assert.That(transaction.Units == 0m).IsTrue();
		await Assert.That(transaction.Price == 0m).IsTrue();
		await Assert.That(transaction.Amount == 0m).IsTrue();
	}

	[Test]
	public async Task Constructor_SetsAllProperties()
	{
		var id = new TransactionId(Guid.NewGuid());
		var date = new DateOnly(2024, 2, 29);
		var assetItemId = new AssetItemId(Guid.NewGuid());
		var transaction = new Transaction(
			id,
			date,
			"Monthly Purchase",
			TransactionType.Buy,
			assetItemId,
			10.5m,
			25.75m,
			270.375m);

		await Assert.That(transaction.Id == id).IsTrue();
		await Assert.That(transaction.Date == date).IsTrue();
		await Assert.That(string.Equals(transaction.Name, "Monthly Purchase", StringComparison.Ordinal)).IsTrue();
		await Assert.That(transaction.TransactionType == TransactionType.Buy).IsTrue();
		await Assert.That(transaction.AssetItemId == assetItemId).IsTrue();
		await Assert.That(transaction.Units == 10.5m).IsTrue();
		await Assert.That(transaction.Price == 25.75m).IsTrue();
		await Assert.That(transaction.Amount == 270.375m).IsTrue();
	}
}
