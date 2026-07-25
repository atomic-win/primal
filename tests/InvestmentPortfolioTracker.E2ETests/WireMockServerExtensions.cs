using System.Globalization;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace InvestmentPortfolioTracker.E2ETests;

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
		server.SetupSymbolSearch(symbol, type: "Equity");
	}

	internal static void SetupEtfSearch(this WireMockServer server, string symbol, string currency = "USD")
	{
		server.SetupSymbolSearch(symbol, type: "ETF", currency: currency);
	}

	internal static void SetupStockSearchEmpty(this WireMockServer server)
	{
		server
			.Given(Request.Create()
				.WithPath("/query")
				.WithParam("function", "SYMBOL_SEARCH")
				.UsingGet())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "text/csv")
				.WithBody("symbol,name,type,region,marketOpen,marketClose,timezone,currency,matchScore\n"));
	}

	internal static void SetupStockPrices(
		this WireMockServer server,
		IReadOnlyCollection<(string Date, decimal Price)> prices)
	{
		var priceRows = string.Join("\n", prices.Select(p =>
			string.Create(CultureInfo.InvariantCulture, $"{p.Date},{p.Price},{p.Price},{p.Price},{p.Price},1000")));

		server
			.Given(Request.Create()
				.WithPath("/query")
				.WithParam("function", "TIME_SERIES_DAILY")
				.UsingGet())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "text/csv")
				.WithBody($"timestamp,open,high,low,close,volume\n{priceRows}\n"));
	}

	internal static void SetupForexRate(
		this WireMockServer server,
		string date,
		decimal closeRate)
	{
		server
			.Given(Request.Create()
				.WithPath("/query")
				.WithParam("function", "FX_DAILY")
				.UsingGet())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "text/csv")
				.WithBody($"timestamp,open,high,low,close\n{date},{closeRate},{closeRate},{closeRate},{closeRate}\n"));
	}

	private static void SetupSymbolSearch(this WireMockServer server, string symbol, string type, string currency = "USD")
	{
		server
			.Given(Request.Create()
				.WithPath("/query")
				.WithParam("function", "SYMBOL_SEARCH")
				.UsingGet())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "text/csv")
				.WithBody($"symbol,name,type,region,marketOpen,marketClose,timezone,currency,matchScore\n{symbol},Apple Inc.,{type},United States,09:30,16:00,UTC-04,{currency},1.0000\n"));
	}
}
