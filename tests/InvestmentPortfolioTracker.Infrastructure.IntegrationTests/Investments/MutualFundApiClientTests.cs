using System.Net;
using InvestmentPortfolioTracker.Infrastructure.Investments;
using RichardSzalay.MockHttp;

namespace InvestmentPortfolioTracker.Infrastructure.IntegrationTests.Investments;

public sealed class MutualFundApiClientTests
{
	[Test]
	public async Task GetOnOrBeforePriceAsync_ThrowsNotSupportedException()
	{
		var client = CreateClient("*", "{}");

		await Assert.ThrowsAsync<NotSupportedException>(
			() => client.GetOnOrBeforePriceAsync("123456", new DateOnly(2024, 5, 31), CancellationToken.None));
	}

	[Test]
	public async Task GetBySymbolAsync_Api404_ReturnsEmptyMutualFund()
	{
		var client = CreateClient("*", string.Empty, HttpStatusCode.NotFound);

		var result = await client.GetBySymbolAsync("999999", CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetBySymbolAsync_ValidResponse_ReturnsMutualFund()
	{
		var json = """
			{
				"meta": {
					"fund_house": "Test Fund House",
					"scheme_type": "Open Ended",
					"scheme_category": "Equity",
					"scheme_code": 119551,
					"scheme_name": "Test Equity Fund"
				},
				"data": [{ "date": "30-05-2026", "nav": "150.25" }],
				"status": "SUCCESS"
			}
			""";
		var client = CreateClient("*", json);

		var result = await client.GetBySymbolAsync("119551", CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetPricesAsync_Api404_ReturnsEmptyDictionary()
	{
		var client = CreateClient("*", string.Empty, HttpStatusCode.NotFound);

		var result = await client.GetPricesAsync("999999", CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetPricesAsync_ValidResponse_ReturnsParsedPrices()
	{
		var json = """
			{
				"meta": { "fund_house": "Test", "scheme_type": "Open", "scheme_category": "Equity", "scheme_code": 119551, "scheme_name": "Test" },
				"data": [
					{ "date": "15-01-2026", "nav": "150.25" },
					{ "date": "16-01-2026", "nav": "151.00" }
				],
				"status": "SUCCESS"
			}
			""";
		var client = CreateClient("*", json);

		var result = await client.GetPricesAsync("119551", CancellationToken.None);

		await Verifier.Verify(result);
	}

	private static MutualFundApiClient CreateClient(string url, string content, HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		var factory = new MockHttpMessageHandler()
			.WithJsonResponse(url, content, statusCode)
			.CreateMockHttpClientFactory<MutualFundApiClient>("https://api.mfapi.in");

		return new MutualFundApiClient(factory);
	}
}
