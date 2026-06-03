using System.Net;
using NSubstitute;
using Primal.Domain.Money;
using Primal.Infrastructure.Investments;
using RichardSzalay.MockHttp;

namespace Primal.Infrastructure.IntegrationTests.Investments;

public sealed class ForexApiClientTests
{
	[Test]
	public async Task GetForexRatesAsync_SameCurrency_ReturnsEmptyDictionary()
	{
		var client = CreateClient("*", "timestamp,open,high,low,close\n");

		var result = await client.GetForexRatesAsync(Currency.USD, Currency.USD, CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetOnOrBeforeForexRateAsync_ThrowsNotSupportedException()
	{
		var client = CreateClient("*", "timestamp,open,high,low,close\n");

		await Assert.ThrowsAsync<NotSupportedException>(
			() => client.GetOnOrBeforeForexRateAsync(Currency.USD, Currency.INR, new DateOnly(2024, 5, 31), CancellationToken.None));
	}

	[Test]
	public async Task GetForexRatesAsync_ValidCsv_ReturnsParsedRates()
	{
		var csv = "timestamp,open,high,low,close\n2026-01-15,83.0,84.0,82.5,83.5\n2026-01-16,83.5,84.5,83.0,84.0\n";
		var client = CreateClient("*", csv);

		var result = await client.GetForexRatesAsync(Currency.INR, Currency.USD, CancellationToken.None);

		await Verifier.Verify(result);
	}

	private static ForexApiClient CreateClient(string url, string content, HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		var factory = new MockHttpMessageHandler()
			.WithCsvResponse(url, content, statusCode)
			.CreateMockHttpClientFactory<ForexApiClient>("https://www.alphavantage.co");

		return new ForexApiClient("test-api-key", factory);
	}
}
