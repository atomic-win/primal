using System.Net;
using NSubstitute;
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
