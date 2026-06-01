using System.Globalization;
using System.Net;
using Dapper;
using Microsoft.Data.Sqlite;
using Primal.Domain.Users;

namespace Primal.E2ETests.CrossCutting;

public sealed class Returns_404_For_Other_Users_AssetItem
{
	[Test]
	public async Task Test()
	{
		// Arrange
		await using var factory = new PrimalE2EFactory();
		_ = factory.CreateClient();

		var userIdA = await factory.CreateUserAsync();
		var clientA = factory.CreateAuthenticatedClient(userIdA);

		var assetItem = await clientA.AddAssetItemAsync(
			name: "User A Bank",
			assetClass: "EmergencyFund",
			assetType: "BankAccount",
			externalId: string.Empty,
			currency: "INR");

		var userIdB = new UserId(Guid.NewGuid());
		var now = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);
		using (var connection = new SqliteConnection($"Data Source={factory.DbPath}"))
		{
			await connection.OpenAsync();
			await connection.ExecuteAsync(
				"""
				INSERT INTO users (Id, Email, FirstName, LastName, FullName, PreferredCurrency, PreferredLocale, CreatedAt, UpdatedAt)
				VALUES (@Id, @Email, @FirstName, @LastName, @FullName, @PreferredCurrency, @PreferredLocale, @CreatedAt, @UpdatedAt)
				""",
				new
				{
					Id = userIdB.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
					Email = "test2@example.com",
					FirstName = "Test",
					LastName = "User2",
					FullName = "Test User2",
					PreferredCurrency = "USD",
					PreferredLocale = "EN_US",
					CreatedAt = now,
					UpdatedAt = now,
				});
		}

		var clientB = factory.CreateAuthenticatedClient(userIdB);

		// Act
		var response = await clientB.GetAsync($"/api/asset-items/{assetItem.Id}");

		// Assert
		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);

		var body = await response.Content.ReadAsStringAsync();
		await Verifier.Verify(body);
	}
}
