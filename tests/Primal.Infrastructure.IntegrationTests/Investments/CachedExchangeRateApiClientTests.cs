using System.Collections.Frozen;
using Microsoft.Extensions.Caching.Hybrid;
using NSubstitute;
using Primal.Application.Investments;
using Primal.Domain.Money;
using Primal.Infrastructure.Investments;

namespace Primal.Infrastructure.IntegrationTests.Investments;

public sealed class CachedExchangeRateApiClientTests
{
	[Test]
	public async Task GetExchangeRatesAsync_SameCurrency_ReturnsEmptyDictionary()
	{
		var hybridCache = Substitute.For<HybridCache>();
		var innerClient = Substitute.For<IExchangeRateApiClient>();
		var client = new CachedExchangeRateApiClient(hybridCache, innerClient);

		var result = await client.GetExchangeRatesAsync(Currency.USD, Currency.USD, CancellationToken.None);

		await Verifier.Verify(result);
		await Assert.That(innerClient.ReceivedCalls().Any()).IsFalse();
	}

	[Test]
	public async Task GetOnOrBeforeExchangeRateAsync_SameCurrency_ReturnsOne()
	{
		var hybridCache = Substitute.For<HybridCache>();
		var innerClient = Substitute.For<IExchangeRateApiClient>();
		var client = new CachedExchangeRateApiClient(hybridCache, innerClient);

		var result = await client.GetOnOrBeforeExchangeRateAsync(Currency.INR, Currency.INR, new DateOnly(2024, 5, 31), CancellationToken.None);

		await Verifier.Verify(result);
		await Assert.That(innerClient.ReceivedCalls().Any()).IsFalse();
	}

	[Test]
	public async Task GetExchangeRatesAsync_DifferentCurrency_FetchesAndCaches()
	{
		var cache = TestCacheHelper.CreateHybridCache();
		var innerClient = Substitute.For<IExchangeRateApiClient>();
		var rates = new Dictionary<DateOnly, decimal> { [new DateOnly(2026, 1, 15)] = 83.5m }
			.ToFrozenDictionary();
		innerClient.GetExchangeRatesAsync(Currency.INR, Currency.USD, Arg.Any<CancellationToken>())
			.Returns(rates);

		var client = new CachedExchangeRateApiClient(cache, innerClient);

		await client.GetExchangeRatesAsync(Currency.INR, Currency.USD, CancellationToken.None);
		await client.GetExchangeRatesAsync(Currency.INR, Currency.USD, CancellationToken.None);

		await innerClient.Received(1).GetExchangeRatesAsync(Currency.INR, Currency.USD, Arg.Any<CancellationToken>());
	}
}
