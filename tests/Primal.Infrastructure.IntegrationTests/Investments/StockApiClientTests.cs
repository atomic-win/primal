using System.Net;
using NSubstitute;
using Primal.Domain.Money;
using Primal.Infrastructure.Investments;
using RichardSzalay.MockHttp;

namespace Primal.Infrastructure.IntegrationTests.Investments;

public sealed class StockApiClientTests
{
	[Test]
	public async Task GetOnOrBeforePriceAsync_ThrowsNotSupportedException()
	{
		var client = CreateClient("*", "[]");

		await Assert.ThrowsAsync<NotSupportedException>(
			() => client.GetOnOrBeforePriceAsync("AAPL", new DateOnly(2024, 5, 31), CancellationToken.None));
	}

	[Test]
	public async Task GetBySymbolAsync_Api404_ReturnsEmptyStock()
	{
		var client = CreateClient("*", string.Empty, HttpStatusCode.NotFound);

		var result = await client.GetBySymbolAsync("INVALID", CancellationToken.None);

		await Assert.That(result.Symbol).IsEqualTo(string.Empty);
		await Assert.That(result.Currency).IsEqualTo(Currency.Unknown);
	}

	[Test]
	public async Task GetBySymbolAsync_EmptyArray_ReturnsEmptyStock()
	{
		var client = CreateClient("*", "[]");

		var result = await client.GetBySymbolAsync("INVALID", CancellationToken.None);

		await Assert.That(result.Symbol).IsEqualTo(string.Empty);
	}

	[Test]
	public async Task GetBySymbolAsync_UnrecognizedCurrency_ReturnsEmptyStock()
	{
		var json = """[{ "symbol": "XYZ", "name": "XYZ Corp", "currency": "XYZ" }]""";
		var client = CreateClient("*", json);

		var result = await client.GetBySymbolAsync("XYZ", CancellationToken.None);

		await Assert.That(result.Symbol).IsEqualTo(string.Empty);
		await Assert.That(result.Currency).IsEqualTo(Currency.Unknown);
	}

	[Test]
	public async Task GetBySymbolAsync_ValidResponse_ReturnsStock()
	{
		var json = """[{ "symbol": "AAPL", "name": "Apple Inc.", "currency": "USD" }]""";
		var client = CreateClient("*", json);

		var result = await client.GetBySymbolAsync("AAPL", CancellationToken.None);

		await Assert.That(result.Symbol).IsEqualTo("AAPL");
		await Assert.That(result.Name).IsEqualTo("Apple Inc.");
		await Assert.That(result.Currency).IsEqualTo(Currency.USD);
	}

	[Test]
	public async Task GetPricesAsync_Api404_ReturnsEmptyDictionary()
	{
		var client = CreateClient("*", string.Empty, HttpStatusCode.NotFound);

		var result = await client.GetPricesAsync("INVALID", CancellationToken.None);

		await Assert.That(result.Count).IsEqualTo(0);
	}

	[Test]
	public async Task GetPricesAsync_EmptyArray_ReturnsEmptyDictionary()
	{
		var client = CreateClient("*", "[]");

		var result = await client.GetPricesAsync("AAPL", CancellationToken.None);

		await Assert.That(result.Count).IsEqualTo(0);
	}

	[Test]
	public async Task GetPricesAsync_ValidResponse_ReturnsParsedPrices()
	{
		var json = """[{ "date": "2026-01-15", "price": 150.50 }, { "date": "2026-01-16", "price": 152.00 }]""";
		var client = CreateClient("*", json);

		var result = await client.GetPricesAsync("AAPL", CancellationToken.None);

		await Assert.That(result.Count).IsEqualTo(2);
		await Assert.That(result[new DateOnly(2026, 1, 15)]).IsEqualTo(150.50m);
		await Assert.That(result[new DateOnly(2026, 1, 16)]).IsEqualTo(152.00m);
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
