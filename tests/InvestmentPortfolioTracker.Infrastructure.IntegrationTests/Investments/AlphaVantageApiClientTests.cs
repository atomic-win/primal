using System.Net;

using RichardSzalay.MockHttp;

using InvestmentPortfolioTracker.Infrastructure.Investments;

namespace InvestmentPortfolioTracker.Infrastructure.IntegrationTests.Investments;

public sealed class AlphaVantageApiClientTests
{
	[Test]
	public async Task GetOnOrBeforePriceAsync_ThrowsNotSupportedException()
	{
		var client = CreateClient("*", string.Empty);

		await Assert.ThrowsAsync<NotSupportedException>(
			() => client.GetOnOrBeforePriceAsync("AAPL", new DateOnly(2024, 5, 31), CancellationToken.None));
	}

	[Test]
	public async Task GetBySymbolAsync_EmptyCsv_ReturnsEmptyStock()
	{
		var csv = "symbol,name,type,region,marketOpen,marketClose,timezone,currency,matchScore\n";
		var client = CreateClient("*", csv);

		var result = await client.GetBySymbolAsync("INVALID", CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetBySymbolAsync_UnrecognizedCurrency_ReturnsEmptyStock()
	{
		var csv = "symbol,name,type,region,marketOpen,marketClose,timezone,currency,matchScore\nXYZ,XYZ Corp,Equity,United States,09:30,16:00,UTC-04,XYZ,1.0000\n";
		var client = CreateClient("*", csv);

		var result = await client.GetBySymbolAsync("XYZ", CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetBySymbolAsync_ValidResponse_ReturnsStock()
	{
		var csv = "symbol,name,type,region,marketOpen,marketClose,timezone,currency,matchScore\nAAPL,Apple Inc.,Equity,United States,09:30,16:00,UTC-04,USD,1.0000\n";
		var client = CreateClient("*", csv);

		var result = await client.GetBySymbolAsync("AAPL", CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetBySymbolAsync_EtfResponse_ReturnsEtf()
	{
		var csv = "symbol,name,type,region,marketOpen,marketClose,timezone,currency,matchScore\nCNDX.LON,iShares NASDAQ 100 UCITS ETF USD (Acc),ETF,United Kingdom,08:00,16:30,UTC+01,USD,1.0000\n";
		var client = CreateClient("*", csv);

		var result = await client.GetBySymbolAsync("CNDX.LON", CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetBySymbolAsync_UnsupportedType_ThrowsNotSupportedException()
	{
		var csv = "symbol,name,type,region,marketOpen,marketClose,timezone,currency,matchScore\nGLD,SPDR Gold Trust,Commodity,United States,09:30,16:00,UTC-04,USD,1.0000\n";
		var client = CreateClient("*", csv);

		await Assert.ThrowsAsync<NotSupportedException>(
			() => client.GetBySymbolAsync("GLD", CancellationToken.None));
	}

	[Test]
	public async Task GetPricesAsync_EmptyCsv_ReturnsEmptyDictionary()
	{
		var csv = "timestamp,open,high,low,close,volume\n";
		var client = CreateClient("*", csv);

		var result = await client.GetPricesAsync("AAPL", CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetPricesAsync_ValidResponse_ReturnsParsedPrices()
	{
		var csv = "timestamp,open,high,low,close,volume\n2026-01-15,149.00,151.00,148.50,150.50,1000\n2026-01-16,151.00,153.00,150.00,152.00,2000\n";
		var client = CreateClient("*", csv);

		var result = await client.GetPricesAsync("AAPL", CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetForexRatesAsync_SameCurrency_ReturnsEmptyDictionary()
	{
		var client = CreateClient("*", string.Empty);

		var result = await client.GetForexRatesAsync(InvestmentPortfolioTracker.Domain.Money.Currency.USD, InvestmentPortfolioTracker.Domain.Money.Currency.USD, CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetOnOrBeforeForexRateAsync_ThrowsNotSupportedException()
	{
		var client = CreateClient("*", string.Empty);

		await Assert.ThrowsAsync<NotSupportedException>(
			() => client.GetOnOrBeforeForexRateAsync(
				InvestmentPortfolioTracker.Domain.Money.Currency.USD,
				InvestmentPortfolioTracker.Domain.Money.Currency.INR,
				new DateOnly(2024, 5, 31),
				CancellationToken.None));
	}

	[Test]
	public async Task GetForexRatesAsync_ValidCsv_ReturnsParsedRates()
	{
		var csv = "timestamp,open,high,low,close\n2026-01-15,83.0,84.0,82.5,83.5\n2026-01-16,83.5,84.5,83.0,84.0\n";
		var client = CreateClient("*", csv);

		var result = await client.GetForexRatesAsync(
			InvestmentPortfolioTracker.Domain.Money.Currency.INR,
			InvestmentPortfolioTracker.Domain.Money.Currency.USD,
			CancellationToken.None);

		await Verifier.Verify(result);
	}

	private static AlphaVantageApiClient CreateClient(string url, string content, HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		var factory = new MockHttpMessageHandler()
			.WithCsvResponse(url, content, statusCode)
			.CreateMockHttpClientFactory<AlphaVantageApiClient>("https://www.alphavantage.co");

		return new AlphaVantageApiClient("test-api-key", factory);
	}
}
