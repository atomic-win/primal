using System.Globalization;
using System.Net.Http.Json;
using Dapper;
using Microsoft.Data.Sqlite;
using Primal.Domain.Users;

namespace Primal.E2ETests;

internal static class TestDataSeeder
{
	internal static async Task<UserId> SeedUserAsync(PrimalE2EFactory factory)
	{
		// User creation requires Google OAuth which cannot be mocked at HTTP level.
		// Direct DB insert is the only option since there's no public user-creation API.
		var userId = new UserId(Guid.NewGuid());
		var now = DateTimeOffset.UtcNow.ToString("O");

		using var connection = new SqliteConnection($"Data Source={factory.DbPath}");
		await connection.OpenAsync();

		await connection.ExecuteAsync(
			"""
			INSERT INTO users (Id, Email, FirstName, LastName, FullName, PreferredCurrency, PreferredLocale, CreatedAt, UpdatedAt)
			VALUES (@Id, @Email, @FirstName, @LastName, @FullName, @PreferredCurrency, @PreferredLocale, @CreatedAt, @UpdatedAt)
			""",
			new
			{
				Id = userId.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant(),
				Email = "test@example.com",
				FirstName = "Test",
				LastName = "User",
				FullName = "Test User",
				PreferredCurrency = "USD",
				PreferredLocale = "EN_US",
				CreatedAt = now,
				UpdatedAt = now,
			});

		return userId;
	}

	internal static async Task<Guid> SeedAssetItemViaMutualFundAsync(HttpClient client)
	{
		var response = await client.PostAsJsonAsync("/api/asset-items", new
		{
			Name = "Test Mutual Fund",
			AssetClass = "Equity",
			AssetType = "MutualFund",
			ExternalId = "119551",
			Currency = "Unknown",
		});

		response.EnsureSuccessStatusCode();

		if (response.Headers.Location is not null)
		{
			return Guid.Parse(response.Headers.Location.ToString().Split('/').Last());
		}

		// Fallback: get the ID from the list endpoint
		var listResponse = await client.GetAsync("/api/asset-items");
		var items = await listResponse.Content.ReadFromJsonAsync<List<AssetItemDto>>();
		return items!.Last().Id;
	}

	internal static async Task<Guid> SeedAssetItemViaFixedDepositAsync(HttpClient client)
	{
		var response = await client.PostAsJsonAsync("/api/asset-items", new
		{
			Name = "Test Fixed Deposit",
			AssetClass = "Debt",
			AssetType = "FixedDeposit",
			ExternalId = string.Empty,
			Currency = "INR",
		});

		response.EnsureSuccessStatusCode();

		if (response.Headers.Location is not null)
		{
			return Guid.Parse(response.Headers.Location.ToString().Split('/').Last());
		}

		var listResponse = await client.GetAsync("/api/asset-items");
		var items = await listResponse.Content.ReadFromJsonAsync<List<AssetItemDto>>();
		return items!.Last().Id;
	}

	internal static async Task<Guid> SeedBuyTransactionAsync(
		HttpClient client, Guid assetItemId, string date, string name, decimal units, decimal price)
	{
		var response = await client.PostAsJsonAsync(
			$"/api/asset-items/{assetItemId}/transactions", new
			{
				AssetItemId = assetItemId,
				Date = date,
				Name = name,
				TransactionType = "Buy",
				Units = units,
				Price = price,
				Amount = 0,
			});

		if (!response.IsSuccessStatusCode)
		{
			var error = await response.Content.ReadAsStringAsync();
			throw new InvalidOperationException($"SeedBuyTransaction failed ({response.StatusCode}): {error}");
		}

		if (response.Headers.Location is not null)
		{
			return Guid.Parse(response.Headers.Location.ToString().Split('/').Last());
		}

		var listResponse = await client.GetAsync(
			$"/api/asset-items/{assetItemId}/transactions?currency=INR");
		var transactions = await listResponse.Content.ReadFromJsonAsync<List<TransactionDto>>();
		return transactions!.Last().Id;
	}

	internal static async Task<Guid> SeedDepositTransactionAsync(
		HttpClient client, Guid assetItemId, string date, string name, decimal amount)
	{
		var response = await client.PostAsJsonAsync(
			$"/api/asset-items/{assetItemId}/transactions", new
			{
				AssetItemId = assetItemId,
				Date = date,
				Name = name,
				TransactionType = "Deposit",
				Units = 0,
				Price = 0,
				Amount = amount,
			});

		response.EnsureSuccessStatusCode();

		if (response.Headers.Location is not null)
		{
			return Guid.Parse(response.Headers.Location.ToString().Split('/').Last());
		}

		var listResponse = await client.GetAsync(
			$"/api/asset-items/{assetItemId}/transactions?currency=INR");
		var transactions = await listResponse.Content.ReadFromJsonAsync<List<TransactionDto>>();
		return transactions!.Last().Id;
	}

	private sealed record AssetItemDto(Guid Id);

	private sealed record TransactionDto(Guid Id);
}
