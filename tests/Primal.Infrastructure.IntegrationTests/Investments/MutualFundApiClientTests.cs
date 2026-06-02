using System.Net;
using NSubstitute;
using Primal.Domain.Money;
using Primal.Infrastructure.Investments;
using RichardSzalay.MockHttp;

namespace Primal.Infrastructure.IntegrationTests.Investments;

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

		await Assert.That(result.SchemeCode).IsEqualTo(string.Empty);
		await Assert.That(result.Currency).IsEqualTo(Currency.Unknown);
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

		await Assert.That(result.SchemeCode).IsEqualTo("119551");
		await Assert.That(result.Name).IsEqualTo("Test Equity Fund");
		await Assert.That(result.Currency).IsEqualTo(Currency.INR);
	}

	[Test]
	public async Task GetPricesAsync_Api404_ReturnsEmptyDictionary()
	{
		var client = CreateClient("*", string.Empty, HttpStatusCode.NotFound);

		var result = await client.GetPricesAsync("999999", CancellationToken.None);

		await Assert.That(result.Count).IsEqualTo(0);
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

		await Assert.That(result.Count).IsEqualTo(2);
		await Assert.That(result[new DateOnly(2026, 1, 15)]).IsEqualTo(150.25m);
		await Assert.That(result[new DateOnly(2026, 1, 16)]).IsEqualTo(151.00m);
	}

	private static MutualFundApiClient CreateClient(string url, string content, HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		var mockHttp = new MockHttpMessageHandler();
		mockHttp.When(url)
			.Respond(statusCode, "application/json", content);

		var httpClient = mockHttp.ToHttpClient();
		httpClient.BaseAddress = new Uri("https://api.mfapi.in");

		var httpClientFactory = Substitute.For<IHttpClientFactory>();
		httpClientFactory.CreateClient(nameof(MutualFundApiClient)).Returns(httpClient);
		return new MutualFundApiClient(httpClientFactory);
	}
}
