using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Primal.E2ETests;

internal static class WireMockServerExtensions
{
	internal static void SetupMutualFundLatest(this WireMockServer server, string schemeCode)
	{
		server
			.Given(Request.Create().WithPath($"/mf/{schemeCode}/latest").UsingGet())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "application/json")
				.WithBody($$"""
				{
					"meta": {
						"fund_house": "Test Fund House",
						"scheme_type": "Open Ended",
						"scheme_category": "Equity",
						"scheme_code": {{schemeCode}},
						"scheme_name": "Test Equity Fund"
					},
					"data": [
						{ "date": "30-05-2026", "nav": "150.25" }
					],
					"status": "SUCCESS"
				}
				"""));
	}

	internal static void SetupMutualFundPrices(
		this WireMockServer server,
		string schemeCode,
		IReadOnlyCollection<(string Date, string Nav)> prices)
	{
		var priceEntries = string.Join(",\n\t\t\t\t\t", prices.Select(p => $$"""{ "date": "{{p.Date}}", "nav": "{{p.Nav}}" }"""));

		server
			.Given(Request.Create().WithPath($"/mf/{schemeCode}").UsingGet())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "application/json")
				.WithBody($$"""
				{
					"meta": {
						"fund_house": "Test Fund House",
						"scheme_type": "Open Ended",
						"scheme_category": "Equity",
						"scheme_code": {{schemeCode}},
						"scheme_name": "Test Equity Fund"
					},
					"data": [
						{{priceEntries}}
					],
					"status": "SUCCESS"
				}
				"""));
	}

	internal static void SetupMutualFundNotFound(this WireMockServer server, string schemeCode)
	{
		server
			.Given(Request.Create().WithPath($"/mf/{schemeCode}/latest").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(404));
	}

	internal static void SetupStockSearch(this WireMockServer server, string symbol)
	{
		server
			.Given(Request.Create().WithPath("/stable/search-symbol").UsingGet())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "application/json")
				.WithBody($$"""
				[
					{
						"symbol": "{{symbol}}",
						"name": "Apple Inc.",
						"currency": "USD"
					}
				]
				"""));
	}

	internal static void SetupStockSearchEmpty(this WireMockServer server)
	{
		server
			.Given(Request.Create().WithPath("/stable/search-symbol").UsingGet())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "application/json")
				.WithBody("[]"));
	}

	internal static void SetupStockPrices(
		this WireMockServer server,
		IReadOnlyCollection<(string Date, decimal Price)> prices)
	{
		var priceEntries = string.Join(",\n\t\t\t\t\t", prices.Select(p => $$"""{ "date": "{{p.Date}}", "price": {{p.Price}} }"""));

		server
			.Given(Request.Create().WithPath("/stable/historical-price-eod/light").UsingGet())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "application/json")
				.WithBody($"[\n\t\t\t\t\t{priceEntries}\n\t\t\t\t]"));
	}

	internal static void SetupForexRate(
		this WireMockServer server,
		string date,
		decimal closeRate)
	{
		server
			.Given(Request.Create().WithPath("/query").UsingGet())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "text/csv")
				.WithBody($"timestamp,open,high,low,close\n{date},{closeRate},{closeRate},{closeRate},{closeRate}\n"));
	}
}
