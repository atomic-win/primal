using System.Net;
using NSubstitute;
using Primal.Domain.Money;
using Primal.Infrastructure.Investments;
using RichardSzalay.MockHttp;

namespace Primal.Infrastructure.IntegrationTests.Investments;

public sealed class StockApiClientTests
{
	[Test]
	public async Task GetBySymbolAsync_NotFound_ReturnsEmptyStock()
	{
		var client = CreateClient("*", "[]", HttpStatusCode.NotFound);

		var result = await client.GetBySymbolAsync("AAPL", CancellationToken.None);

		await Assert.That(result.Symbol).IsEqualTo(string.Empty);
		await Assert.That(result.Name).IsEqualTo(string.Empty);
		await Assert.That(result.Currency).IsEqualTo(Currency.Unknown);
	}

	[Test]
	public async Task GetBySymbolAsync_EmptyArray_ReturnsEmptyStock()
	{
		var client = CreateClient("*", "[]");

		var result = await client.GetBySymbolAsync("AAPL", CancellationToken.None);

		await Assert.That(result.Symbol).IsEqualTo(string.Empty);
		await Assert.That(result.Name).IsEqualTo(string.Empty);
		await Assert.That(result.Currency).IsEqualTo(Currency.Unknown);
	}

	[Test]
	public async Task GetBySymbolAsync_InvalidCurrency_ReturnsEmptyStock()
	{
		const string json = """
		[{"symbol":"AAPL","name":"Apple Inc.","currency":"INVALID"}]
		""";
		var client = CreateClient("*", json);

		var result = await client.GetBySymbolAsync("AAPL", CancellationToken.None);

		await Assert.That(result.Symbol).IsEqualTo(string.Empty);
		await Assert.That(result.Name).IsEqualTo(string.Empty);
		await Assert.That(result.Currency).IsEqualTo(Currency.Unknown);
	}

	[Test]
	public async Task GetBySymbolAsync_Success_ReturnsStock()
	{
		const string json = """
		[{"symbol":"AAPL","name":"Apple Inc.","currency":"USD"}]
		""";
		var client = CreateClient("/stable/search-symbol*", json);

		var result = await client.GetBySymbolAsync("AAPL", CancellationToken.None);

		await Assert.That(result.Symbol).IsEqualTo("AAPL");
		await Assert.That(result.Name).IsEqualTo("Apple Inc.");
		await Assert.That(result.Currency).IsEqualTo(Currency.USD);
	}

	[Test]
	public async Task GetPricesAsync_NotFound_ReturnsEmptyDictionary()
	{
		var client = CreateClient("*", "[]", HttpStatusCode.NotFound);

		var result = await client.GetPricesAsync("AAPL", CancellationToken.None);

		await Assert.That(result.Count).IsEqualTo(0);
	}

	[Test]
	public async Task GetPricesAsync_Success_ReturnsParsedPrices()
	{
		const string json = """
		[{"date":"2024-05-31","price":192.25},{"date":"2024-05-30","price":191.50}]
		""";
		var client = CreateClient("/stable/historical-price-eod*", json);

		var result = await client.GetPricesAsync("AAPL", CancellationToken.None);

		await Assert.That(result.Count).IsEqualTo(2);
		await Assert.That(result[new DateOnly(2024, 5, 31)]).IsEqualTo(192.25m);
		await Assert.That(result[new DateOnly(2024, 5, 30)]).IsEqualTo(191.50m);
	}

	[Test]
	public async Task GetOnOrBeforePriceAsync_ThrowsNotSupportedException()
	{
		var client = CreateClient("*", "[]");

		await Assert.ThrowsAsync<NotSupportedException>(
			() => client.GetOnOrBeforePriceAsync("AAPL", new DateOnly(2024, 5, 31), CancellationToken.None));
	}

	private static StockApiClient CreateClient(string url, string content, HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		var mockHttp = new MockHttpMessageHandler();
		mockHttp.When(url)
			.Respond(statusCode, "application/json", content);

		var httpClient = mockHttp.ToHttpClient();
		httpClient.BaseAddress = new Uri("https://financialmodelingprep.com");

		var httpClientFactory = Substitute.For<IHttpClientFactory>();
		httpClientFactory.CreateClient(nameof(StockApiClient)).Returns(httpClient);
		return new StockApiClient("test-api-key", httpClientFactory);
	}
}
