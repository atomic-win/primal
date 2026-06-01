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
}
