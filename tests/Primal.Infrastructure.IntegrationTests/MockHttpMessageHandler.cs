using System.Net;
using System.Net.Http;
using System.Text;

namespace Primal.Infrastructure.IntegrationTests;

internal sealed class MockHttpMessageHandler : HttpMessageHandler
{
	private readonly HttpResponseMessage response;

	internal MockHttpMessageHandler(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		this.response = new HttpResponseMessage(statusCode)
		{
			Content = new StringContent(content, Encoding.UTF8, "application/json"),
		};
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		return Task.FromResult(this.response);
	}
}
