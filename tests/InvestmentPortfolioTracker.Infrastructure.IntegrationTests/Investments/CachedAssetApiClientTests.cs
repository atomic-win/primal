using System.Collections.Frozen;
using InvestmentPortfolioTracker.Core.Investments;
using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Money;
using InvestmentPortfolioTracker.Infrastructure.Investments;
using NSubstitute;

namespace InvestmentPortfolioTracker.Infrastructure.IntegrationTests.Investments;

public sealed class CachedAssetApiClientTests
{
	[Test]
	public async Task GetBySymbol_SecondCall_ReturnsFromCache()
	{
		var cache = TestCacheFactory.CreateHybridCache();
		var inner = Substitute.For<IAssetApiClient<MutualFund>>();
		inner.GetBySymbolAsync("119551", Arg.Any<CancellationToken>())
			.Returns(new MutualFund("119551", "Test Fund", "Open", "Equity", Currency.INR));

		var client = new CachedAssetApiClient<MutualFund>(cache, inner, CreateRateRepository(), RateType.MutualFund);

		await client.GetBySymbolAsync("119551", CancellationToken.None);
		await client.GetBySymbolAsync("119551", CancellationToken.None);

		await inner.Received(1).GetBySymbolAsync("119551", Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task GetPrices_SecondCall_ReturnsFromCache()
	{
		var cache = TestCacheFactory.CreateHybridCache();
		var inner = Substitute.For<IAssetApiClient<MutualFund>>();
		var prices = new Dictionary<DateOnly, decimal> { [new DateOnly(2026, 1, 15)] = 150.25m }.ToFrozenDictionary();
		inner.GetPricesAsync("119551", Arg.Any<CancellationToken>()).Returns(prices);

		var client = new CachedAssetApiClient<MutualFund>(cache, inner, CreateRateRepository(), RateType.MutualFund);

		await client.GetPricesAsync("119551", CancellationToken.None);
		await client.GetPricesAsync("119551", CancellationToken.None);

		await inner.Received(1).GetPricesAsync("119551", Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task GetOnOrBeforePrice_SecondCall_ReturnsFromCache()
	{
		var cache = TestCacheFactory.CreateHybridCache();
		var inner = Substitute.For<IAssetApiClient<MutualFund>>();
		var prices = new Dictionary<DateOnly, decimal> { [new DateOnly(2026, 1, 15)] = 150.25m }.ToFrozenDictionary();
		inner.GetPricesAsync("119551", Arg.Any<CancellationToken>()).Returns(prices);

		var timeProvider = new Microsoft.Extensions.Time.Testing.FakeTimeProvider(
			new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero));
		var client = new CachedAssetApiClient<MutualFund>(cache, inner, CreateRateRepository(timeProvider), RateType.MutualFund);

		var first = await client.GetOnOrBeforePriceAsync("119551", new DateOnly(2026, 1, 15), CancellationToken.None);
		var second = await client.GetOnOrBeforePriceAsync("119551", new DateOnly(2026, 1, 15), CancellationToken.None);

		await Verifier.Verify(new { first, second });
	}

	private static RateRepository CreateRateRepository(TimeProvider timeProvider = null)
	{
		return new RateRepository(TestDbFactory.CreateTestDatabase(), timeProvider ?? TimeProvider.System);
	}
}
