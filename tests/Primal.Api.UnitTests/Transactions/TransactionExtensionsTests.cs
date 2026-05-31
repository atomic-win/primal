using NSubstitute;
using Primal.Api.Transactions;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.UnitTests.Transactions;

public sealed class TransactionExtensionsTests
{
	[Test]
	public async Task ToResponse_MapsAllFieldsCorrectly()
	{
		var transactionId = new TransactionId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());
		var userId = new UserId(Guid.NewGuid());
		var date = new DateOnly(2024, 3, 15);

		var transaction = new Transaction(
			transactionId,
			date,
			"Test Transaction",
			TransactionType.Buy,
			assetItemId,
			units: 10m,
			price: 100m,
			amount: 0m);

		var calculator = Substitute.For<ITransactionAmountCalculator>();
		calculator.CalculateAmountAsync(
			userId, transaction, date, Currency.INR, Arg.Any<CancellationToken>())
			.Returns(1000m);

		var response = await transaction.ToResponse(
			userId,
			calculator,
			Currency.INR,
			CancellationToken.None);

		await Assert.That(response.Id).IsEqualTo(transactionId.Value);
		await Assert.That(response.Date).IsEqualTo(date);
		await Assert.That(response.Name).IsEqualTo("Test Transaction");
		await Assert.That(response.TransactionType).IsEqualTo(TransactionType.Buy);
		await Assert.That(response.AssetItemId).IsEqualTo(assetItemId.Value);
		await Assert.That(response.Units).IsEqualTo(10m);
		await Assert.That(response.Price).IsEqualTo(100m);
		await Assert.That(response.Amount).IsEqualTo(1000m);
	}
}
