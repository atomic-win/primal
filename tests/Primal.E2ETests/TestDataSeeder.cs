using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Primal.Api.AssetItems;
using Primal.Api.Transactions;

namespace Primal.E2ETests;

internal static class TestDataSeeder
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() },
	};

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
		var result = await response.Content.ReadFromJsonAsync<AssetItemResponse>(JsonOptions);
		return result!.Id;
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
		var result = await response.Content.ReadFromJsonAsync<AssetItemResponse>(JsonOptions);
		return result!.Id;
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

		var result = await response.Content.ReadFromJsonAsync<TransactionResponse>(JsonOptions);
		return result!.Id;
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
		var result = await response.Content.ReadFromJsonAsync<TransactionResponse>(JsonOptions);
		return result!.Id;
	}
}
