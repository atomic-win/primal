using InvestmentPortfolioTracker.Api.Transactions;
using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Money;

namespace InvestmentPortfolioTracker.Api.UnitTests.Transactions;

public sealed class TransactionValidationExtensionsTests
{
	[Test]
	public void IsValidForAssetType_ThrowsForUnknownAssetType()
	{
		var asset = new Asset(
			new AssetId(Guid.NewGuid()),
			"Test",
			AssetClass.Unknown,
			AssetType.Unknown,
			Currency.Unknown,
			string.Empty);

		Assert.Throws<InvalidOperationException>(
			() => TransactionValidationExtensions.IsValidForAssetType(TransactionType.Buy, asset));
	}
}
