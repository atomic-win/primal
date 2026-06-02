using System.Net;
using NSubstitute;
using Primal.Domain.Money;
using Primal.Infrastructure.Investments;
using RichardSzalay.MockHttp;

namespace Primal.Infrastructure.IntegrationTests.Investments;

public sealed class ExchangeRateApiClientTests
{
	[Test]
	public async Task GetExchangeRatesAsync_SameCurrency_ReturnsEmptyDictionary()
	{
		var client = CreateClient("*", "timestamp,open,high,low,close\n");

		var result = await client.GetExchangeRatesAsync(Currency.USD, Currency.USD, CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetOnOrBeforeExchangeRateAsync_ThrowsNotSupportedException()
	{
		var client = CreateClient("*", "timestamp,open,high,low,close\n");

		await Assert.ThrowsAsync<NotSupportedException>(
			() => client.GetOnOrBeforeExchangeRateAsync(Currency.USD, Currency.INR, new DateOnly(2024, 5, 31), CancellationToken.None));
	}

	[Test]
	public async Task GetExchangeRatesAsync_ValidCsv_ReturnsParsedRates()
	{
		var csv = "timestamp,open,high,low,close\n2026-01-15,83.0,84.0,82.5,83.5\n2026-01-16,83.5,84.5,83.0,84.0\n";
		var client = CreateClient("*", csv, "text/csv");

		var result = await client.GetExchangeRatesAsync(Currency.INR, Currency.USD, CancellationToken.None);

		await Verifier.Verify(result);
	}

	private static ExchangeRateApiClient CreateClient(string url, string content, string mediaType = "text/csv", HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		var mockHttp = new MockHttpMessageHandler();
		mockHttp.When(url)
			.Respond(statusCode, mediaType, content);

		var httpClient = mockHttp.ToHttpClient();
		httpClient.BaseAddress = new Uri("https://www.alphavantage.co");

		var httpClientFactory = Substitute.For<IHttpClientFactory>();
		httpClientFactory.CreateClient(nameof(ExchangeRateApiClient)).Returns(httpClient);
		return new ExchangeRateApiClient("test-api-key", httpClientFactory);
	}
}
