using System.Net;
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

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetBySymbolAsync_EmptyArray_ReturnsEmptyStock()
	{
		var client = CreateClient("*", "[]");

		var result = await client.GetBySymbolAsync("INVALID", CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetBySymbolAsync_UnrecognizedCurrency_ReturnsEmptyStock()
	{
		var json = """[{ "symbol": "XYZ", "name": "XYZ Corp", "currency": "XYZ" }]""";
		var client = CreateClient("*", json);

		var result = await client.GetBySymbolAsync("XYZ", CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetBySymbolAsync_ValidResponse_ReturnsStock()
	{
		var json = """[{ "symbol": "AAPL", "name": "Apple Inc.", "currency": "USD" }]""";
		var client = CreateClient("*", json);

		var result = await client.GetBySymbolAsync("AAPL", CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetPricesAsync_Api404_ReturnsEmptyDictionary()
	{
		var client = CreateClient("*", string.Empty, HttpStatusCode.NotFound);

		var result = await client.GetPricesAsync("INVALID", CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetPricesAsync_EmptyArray_ReturnsEmptyDictionary()
	{
		var client = CreateClient("*", "[]");

		var result = await client.GetPricesAsync("AAPL", CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetPricesAsync_ValidResponse_ReturnsParsedPrices()
	{
		var json = """[{ "date": "2026-01-15", "price": 150.50 }, { "date": "2026-01-16", "price": 152.00 }]""";
		var client = CreateClient("*", json);

		var result = await client.GetPricesAsync("AAPL", CancellationToken.None);

		await Verifier.Verify(result);
	}

	private static StockApiClient CreateClient(string url, string content, HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		var factory = new MockHttpMessageHandler()
			.WithJsonResponse(url, content, statusCode)
			.CreateMockHttpClientFactory<StockApiClient>("https://financialmodelingprep.com");

		return new StockApiClient("test-api-key", factory);
	}
}
