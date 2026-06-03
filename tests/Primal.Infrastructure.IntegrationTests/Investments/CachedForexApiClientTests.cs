using System.Collections.Frozen;
using NSubstitute;
using Primal.Application.Investments;
using Primal.Domain.Money;
using Primal.Infrastructure.Investments;

namespace Primal.Infrastructure.IntegrationTests.Investments;

public sealed class CachedForexApiClientTests
{
	[Test]
	public async Task GetForexRatesAsync_SameCurrency_ReturnsEmptyDictionary()
	{
		var cache = TestCacheFactory.CreateHybridCache();
		var innerClient = Substitute.For<IForexApiClient>();
		var client = new CachedForexApiClient(cache, innerClient, CreateRateRepository());

		var result = await client.GetForexRatesAsync(Currency.USD, Currency.USD, CancellationToken.None);

		await Verifier.Verify(result);
		await innerClient.DidNotReceive().GetForexRatesAsync(Arg.Any<Currency>(), Arg.Any<Currency>(), Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task GetOnOrBeforeForexRateAsync_SameCurrency_ReturnsOne()
	{
		var cache = TestCacheFactory.CreateHybridCache();
		var innerClient = Substitute.For<IForexApiClient>();
		var client = new CachedForexApiClient(cache, innerClient, CreateRateRepository());

		var result = await client.GetOnOrBeforeForexRateAsync(Currency.INR, Currency.INR, new DateOnly(2024, 5, 31), CancellationToken.None);

		await Verifier.Verify(result);
		await innerClient.DidNotReceive().GetForexRatesAsync(Arg.Any<Currency>(), Arg.Any<Currency>(), Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task GetForexRatesAsync_DifferentCurrency_FetchesAndCaches()
	{
		var cache = TestCacheFactory.CreateHybridCache();
		var innerClient = Substitute.For<IForexApiClient>();
		var rates = new Dictionary<DateOnly, decimal> { [new DateOnly(2026, 1, 15)] = 83.5m }
			.ToFrozenDictionary();
		innerClient.GetForexRatesAsync(Currency.INR, Currency.USD, Arg.Any<CancellationToken>())
			.Returns(rates);

		var client = new CachedForexApiClient(cache, innerClient, CreateRateRepository());

		await client.GetForexRatesAsync(Currency.INR, Currency.USD, CancellationToken.None);
		await client.GetForexRatesAsync(Currency.INR, Currency.USD, CancellationToken.None);

		await innerClient.Received(1).GetForexRatesAsync(Currency.INR, Currency.USD, Arg.Any<CancellationToken>());
	}

	private static RateRepository CreateRateRepository()
	{
		return new RateRepository(TestDbFactory.CreateTestDatabase(), TimeProvider.System);
	}
}
