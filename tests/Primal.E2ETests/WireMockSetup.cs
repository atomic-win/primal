using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Primal.E2ETests;

internal static class WireMockSetup
{
	internal static void SetupMutualFundLatest(WireMockServer server, string schemeCode = "119551")
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

	internal static void SetupMutualFundPrices(WireMockServer server, string schemeCode = "119551")
	{
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
						{ "date": "15-01-2026", "nav": "150.25" },
						{ "date": "16-01-2026", "nav": "151.00" }
					],
					"status": "SUCCESS"
				}
				"""));
	}

	internal static void SetupMutualFundNotFound(WireMockServer server, string schemeCode)
	{
		server
			.Given(Request.Create().WithPath($"/mf/{schemeCode}/latest").UsingGet())
			.RespondWith(Response.Create().WithStatusCode(404));
	}

	internal static void SetupStockSearch(WireMockServer server, string symbol = "AAPL")
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

	internal static void SetupStockSearchEmpty(WireMockServer server)
	{
		server
			.Given(Request.Create().WithPath("/stable/search-symbol").UsingGet())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "application/json")
				.WithBody("[]"));
	}
}
