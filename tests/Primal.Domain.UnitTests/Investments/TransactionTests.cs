using Primal.Domain.Investments;

namespace Primal.Domain.UnitTests.Investments;

public sealed class TransactionTests
{
	[Test]
	public async Task Empty_ReturnsExpectedDefaultValues()
	{
		var transaction = Transaction.Empty;

		await Verifier.Verify(transaction);
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

		await Verifier.Verify(transaction);
	}
}
