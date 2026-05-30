using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using NSubstitute;
using Primal.Api.Users;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.IntegrationTests.Users;

public sealed class GetUserEndpointTests
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() },
	};

	[Test]
	public async Task GetUser_Unauthenticated_Returns401()
	{
		await using var factory = new PrimalApiFactory();
		var client = factory.CreateClient();

		var response = await client.GetAsync("/api/users/me");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
	}

	[Test]
	public async Task GetUser_UserNotFound_Returns404()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());

		factory.UserRepository
			.GetUserAsync(userId, Arg.Any<CancellationToken>())
			.Returns(User.Empty);

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.GetAsync("/api/users/me");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}

	[Test]
	public async Task GetUser_UserExists_ReturnsUserResponse()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var user = new User(
			userId,
			"ada@example.com",
			"Ada",
			"Lovelace",
			"Ada Lovelace",
			Currency.USD,
			Locale.EN_US);

		factory.UserRepository
			.GetUserAsync(userId, Arg.Any<CancellationToken>())
			.Returns(user);

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.GetAsync("/api/users/me");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadFromJsonAsync<UserResponse>(JsonOptions);
		await Assert.That(body!.Id).IsEqualTo(userId.Value);
		await Assert.That(body.Email).IsEqualTo("ada@example.com");
		await Assert.That(body.FirstName).IsEqualTo("Ada");
		await Assert.That(body.LastName).IsEqualTo("Lovelace");
		await Assert.That(body.FullName).IsEqualTo("Ada Lovelace");
		await Assert.That(body.PreferredCurrency).IsEqualTo(Currency.USD);
		await Assert.That(body.PreferredLocale).IsEqualTo(Locale.EN_US);
	}
}
