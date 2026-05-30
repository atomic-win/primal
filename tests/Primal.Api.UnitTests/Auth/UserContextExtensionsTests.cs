using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Primal.Api;
using Primal.Domain.Users;

namespace Primal.Api.UnitTests.Auth;

public sealed class UserContextExtensionsTests
{
	[Test]
	public async Task GetUserId_FromHttpContextAccessor_ReturnsUserId()
	{
		var expectedId = Guid.NewGuid();
		var accessor = CreateHttpContextAccessor(expectedId);

		var result = accessor.GetUserId();

		await Assert.That(result).IsEqualTo(new UserId(expectedId));
	}

	[Test]
	public void GetUserId_MissingClaim_ThrowsInvalidOperationException()
	{
		var httpContext = new DefaultHttpContext
		{
			User = new ClaimsPrincipal(new ClaimsIdentity()),
		};
		var accessor = Substitute.For<IHttpContextAccessor>();
		accessor.HttpContext.Returns(httpContext);

		Assert.Throws<InvalidOperationException>(
			() => accessor.GetUserId());
	}

	private static IHttpContextAccessor CreateHttpContextAccessor(Guid userId)
	{
		var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
		var identity = new ClaimsIdentity(claims);
		var principal = new ClaimsPrincipal(identity);
		var httpContext = new DefaultHttpContext { User = principal };
		var accessor = Substitute.For<IHttpContextAccessor>();
		accessor.HttpContext.Returns(httpContext);
		return accessor;
	}
}
