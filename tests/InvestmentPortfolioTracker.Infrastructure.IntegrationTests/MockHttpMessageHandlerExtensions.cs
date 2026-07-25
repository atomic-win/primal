using System.Net;
using NSubstitute;
using RichardSzalay.MockHttp;

namespace InvestmentPortfolioTracker.Infrastructure.IntegrationTests;

internal static class MockHttpMessageHandlerExtensions
{
	internal static IHttpClientFactory CreateMockHttpClientFactory<T>(
		this MockHttpMessageHandler mockHttp,
		string baseAddress)
	{
		var httpClient = mockHttp.ToHttpClient();
		httpClient.BaseAddress = new Uri(baseAddress);

		var httpClientFactory = Substitute.For<IHttpClientFactory>();
		httpClientFactory.CreateClient(typeof(T).Name).Returns(httpClient);

		return httpClientFactory;
	}

	internal static MockHttpMessageHandler WithJsonResponse(
		this MockHttpMessageHandler mockHttp,
		string url,
		string content,
		HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		mockHttp.When(url)
			.Respond(statusCode, "application/json", content);

		return mockHttp;
	}

	internal static MockHttpMessageHandler WithCsvResponse(
		this MockHttpMessageHandler mockHttp,
		string url,
		string content,
		HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		mockHttp.When(url)
			.Respond(statusCode, "text/csv", content);

		return mockHttp;
	}
}
