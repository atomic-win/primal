using System.Globalization;
using System.Net;
using System.Net.Http.Json;

using Dapper;
using Microsoft.Data.Sqlite;
using NSubstitute;

using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.E2ETests.Auth.GoogleLogin;

public sealed class Returns_200_When_Existing_User_Logs_In
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new InvestmentPortfolioTrackerE2EFactory();
		var client = factory.CreateClient();

		var userId = await factory.CreateUserAsync();

		// Insert user_id mapping for Google provider
		using (var connection = new SqliteConnection($"Data Source={factory.DbPath}"))
		{
			await connection.OpenAsync();
			await connection.ExecuteAsync(
				"""
				INSERT INTO user_ids (Id, IdentityProvider, UserId, CreatedAt, UpdatedAt)
				VALUES (@Id, @IdentityProvider, @UserId, @CreatedAt, @UpdatedAt)
				""",
				new
				{
					Id = "existing-google-subject",
					IdentityProvider = "Google",
					UserId = userId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
					CreatedAt = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
					UpdatedAt = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
				});
		}

		factory.IdTokenValidator
			.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(new IdentityProviderUser(
				id: new IdentityProviderUserId("existing-google-subject"),
				identityProvider: IdentityProvider.Google,
				email: "test@example.com",
				firstName: "Test",
				lastName: "User",
				fullName: "Test User",
				profilePictureUrl: new Uri("https://example.com/photo.jpg")));

		// Act
		var response = await client.PostAsJsonAsync("/api/auth/login/google", new
		{
			IdToken = "valid-google-token",
		});

		// Assert
		var body = await response.Content.ReadAsStringAsync();
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		await Verifier.Verify(body);
	}
}
