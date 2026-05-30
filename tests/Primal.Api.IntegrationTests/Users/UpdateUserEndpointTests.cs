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

public sealed class UpdateUserEndpointTests
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() },
	};

	[Test]
	public async Task UpdateUser_Unauthenticated_Returns401()
	{
		await using var factory = new PrimalApiFactory();
		var client = factory.CreateClient();

		var response = await client.PatchAsJsonAsync("/api/users/me", new UpdateUserRequest(Currency.INR, Locale.Unknown), JsonOptions);

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
	}

	[Test]
	public async Task UpdateUser_UserNotFound_Returns404()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());

		factory.UserRepository
			.GetUserAsync(userId, Arg.Any<CancellationToken>())
			.Returns(User.Empty);

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.PatchAsJsonAsync("/api/users/me", new UpdateUserRequest(Currency.INR, Locale.Unknown), JsonOptions);

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}

	[Test]
	public async Task UpdateUser_NoChange_Returns204WithoutUpdate()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var user = new User(userId, "ada@example.com", "Ada", "Lovelace", "Ada Lovelace", Currency.USD, Locale.EN_US);

		factory.UserRepository
			.GetUserAsync(userId, Arg.Any<CancellationToken>())
			.Returns(user);

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.PatchAsJsonAsync("/api/users/me", new UpdateUserRequest(Currency.USD, Locale.EN_US), JsonOptions);

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
		await factory.UserRepository.DidNotReceive().UpdateUserProfileAsync(
			Arg.Any<UserId>(), Arg.Any<Currency>(), Arg.Any<Locale>(), Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task UpdateUser_ChangeCurrency_Returns204AndUpdates()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var user = new User(userId, "ada@example.com", "Ada", "Lovelace", "Ada Lovelace", Currency.USD, Locale.EN_US);

		factory.UserRepository
			.GetUserAsync(userId, Arg.Any<CancellationToken>())
			.Returns(user);

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.PatchAsJsonAsync("/api/users/me", new UpdateUserRequest(Currency.INR, Locale.Unknown), JsonOptions);

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
		await factory.UserRepository.Received(1).UpdateUserProfileAsync(
			userId, Currency.INR, Locale.EN_US, Arg.Any<CancellationToken>());
	}
}
