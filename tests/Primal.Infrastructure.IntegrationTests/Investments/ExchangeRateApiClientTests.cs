using System.Net;
using NSubstitute;
using Primal.Domain.Money;
using Primal.Infrastructure.Investments;

namespace Primal.Infrastructure.IntegrationTests.Investments;

public sealed class ExchangeRateApiClientTests
{
	[Test]
	public async Task GetExchangeRatesAsync_SameCurrency_ReturnsEmptyDictionary()
	{
		var client = CreateClient("timestamp,open,high,low,close\n");

		var result = await client.GetExchangeRatesAsync(Currency.USD, Currency.USD, CancellationToken.None);

		await Assert.That(result.Count).IsEqualTo(0);
	}

	[Test]
	public async Task GetExchangeRatesAsync_Success_ReturnsParsedRates()
	{
		const string csv = """
		timestamp,open,high,low,close
		2024-05-31,83.1000,83.5000,83.0500,83.3500
		2024-05-30,83.0000,83.2000,82.9000,83.1000
		""";
		var client = CreateClient(csv);

		var result = await client.GetExchangeRatesAsync(Currency.USD, Currency.INR, CancellationToken.None);

		await Assert.That(result.Count).IsEqualTo(2);
		await Assert.That(result[new DateOnly(2024, 5, 31)]).IsEqualTo(83.3500m);
		await Assert.That(result[new DateOnly(2024, 5, 30)]).IsEqualTo(83.1000m);
	}

	[Test]
	public async Task GetOnOrBeforeExchangeRateAsync_ThrowsNotSupportedException()
	{
		var client = CreateClient("timestamp,open,high,low,close\n");

		await Assert.ThrowsAsync<NotSupportedException>(
			() => client.GetOnOrBeforeExchangeRateAsync(Currency.USD, Currency.INR, new DateOnly(2024, 5, 31), CancellationToken.None));
	}

	private static ExchangeRateApiClient CreateClient(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		var httpClient = new HttpClient(new MockHttpMessageHandler(content, statusCode))
		{
			BaseAddress = new Uri("https://test.com"),
		};
		var httpClientFactory = Substitute.For<IHttpClientFactory>();
		httpClientFactory.CreateClient(nameof(ExchangeRateApiClient)).Returns(httpClient);
		return new ExchangeRateApiClient("test-api-key", httpClientFactory);
	}
}
