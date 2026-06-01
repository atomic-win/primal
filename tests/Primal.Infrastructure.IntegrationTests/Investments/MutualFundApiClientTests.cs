using System.Net;
using NSubstitute;
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
