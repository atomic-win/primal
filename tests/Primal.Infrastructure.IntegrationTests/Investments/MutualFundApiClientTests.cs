using System.Net;
using NSubstitute;
using Primal.Domain.Money;
using Primal.Infrastructure.Investments;

namespace Primal.Infrastructure.IntegrationTests.Investments;

public sealed class MutualFundApiClientTests
{
	[Test]
	public async Task GetBySymbolAsync_NotFound_ReturnsEmptyMutualFund()
	{
		var client = CreateClient("{}", HttpStatusCode.NotFound);

		var result = await client.GetBySymbolAsync("123456", CancellationToken.None);

		await Assert.That(result.SchemeCode).IsEqualTo(string.Empty);
		await Assert.That(result.Name).IsEqualTo(string.Empty);
		await Assert.That(result.SchemeType).IsEqualTo(string.Empty);
		await Assert.That(result.SchemeCategory).IsEqualTo(string.Empty);
		await Assert.That(result.Currency).IsEqualTo(Currency.Unknown);
	}

	[Test]
	public async Task GetBySymbolAsync_Success_ReturnsMutualFund()
	{
		const string json = """
		{"meta":{"fund_house":"Test Fund","scheme_type":"Open Ended","scheme_category":"Equity","scheme_code":123456,"scheme_name":"Test Scheme"},"data":[{"date":"31-05-2024","nav":"150.25"}],"status":"SUCCESS"}
		""";
		var client = CreateClient(json);

		var result = await client.GetBySymbolAsync("123456", CancellationToken.None);

		await Assert.That(result.SchemeCode).IsEqualTo("123456");
		await Assert.That(result.Name).IsEqualTo("Test Scheme");
		await Assert.That(result.SchemeType).IsEqualTo("Open Ended");
		await Assert.That(result.SchemeCategory).IsEqualTo("Equity");
		await Assert.That(result.Currency).IsEqualTo(Currency.INR);
	}

	[Test]
	public async Task GetPricesAsync_NotFound_ReturnsEmptyDictionary()
	{
		var client = CreateClient("{}", HttpStatusCode.NotFound);

		var result = await client.GetPricesAsync("123456", CancellationToken.None);

		await Assert.That(result.Count).IsEqualTo(0);
	}

	[Test]
	public async Task GetPricesAsync_Success_ReturnsParsedPrices()
	{
		const string json = """
		{"meta":{"fund_house":"Test Fund","scheme_type":"Open Ended","scheme_category":"Equity","scheme_code":123456,"scheme_name":"Test Scheme"},"data":[{"date":"31-05-2024","nav":"150.25"},{"date":"30-05-2024","nav":"149.75"}],"status":"SUCCESS"}
		""";
		var client = CreateClient(json);

		var result = await client.GetPricesAsync("123456", CancellationToken.None);

		await Assert.That(result.Count).IsEqualTo(2);
		await Assert.That(result[new DateOnly(2024, 5, 31)]).IsEqualTo(150.25m);
		await Assert.That(result[new DateOnly(2024, 5, 30)]).IsEqualTo(149.75m);
	}

	[Test]
	public async Task GetOnOrBeforePriceAsync_ThrowsNotSupportedException()
	{
		var client = CreateClient("{}");

		await Assert.ThrowsAsync<NotSupportedException>(
			() => client.GetOnOrBeforePriceAsync("123456", new DateOnly(2024, 5, 31), CancellationToken.None));
	}

	private static MutualFundApiClient CreateClient(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		var httpClient = new HttpClient(new MockHttpMessageHandler(content, statusCode))
		{
			BaseAddress = new Uri("https://test.com"),
		};
		var httpClientFactory = Substitute.For<IHttpClientFactory>();
		httpClientFactory.CreateClient(nameof(MutualFundApiClient)).Returns(httpClient);
		return new MutualFundApiClient(httpClientFactory);
	}
}
