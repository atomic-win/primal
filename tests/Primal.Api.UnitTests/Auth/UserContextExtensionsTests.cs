using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Primal.Domain.Users;

namespace Primal.Api.UnitTests.Auth;

public sealed class UserContextExtensionsTests
{
	[Test]
	public async Task GetUserId_FromEndpoint_ReturnsUserId()
	{
		var expectedId = Guid.NewGuid();
		var claims = new[] { new Claim(ClaimTypes.NameIdentifier, expectedId.ToString()) };
		var identity = new ClaimsIdentity(claims);
		var principal = new ClaimsPrincipal(identity);

		var endpoint = FastEndpoints.Factory.Create<TestEndpoint>(httpContext: new DefaultHttpContext { User = principal });

		var result = endpoint.GetUserId();

		await Assert.That(result).IsEqualTo(new UserId(expectedId));
	}

	[Test]
	public void GetUserId_MissingClaim_ThrowsInvalidOperationException()
	{
		var endpoint = FastEndpoints.Factory.Create<TestEndpoint>(
			httpContext: new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) });

		Assert.Throws<InvalidOperationException>(
			() => endpoint.GetUserId());
	}

	internal sealed class TestEndpoint : FastEndpoints.Endpoint<TestRequest, object>
	{
		public override void Configure()
		{
			this.Get("/test");
			this.AllowAnonymous();
		}

		public override Task HandleAsync(TestRequest req, CancellationToken ct) => Task.CompletedTask;
	}

	internal sealed record TestRequest;
}
