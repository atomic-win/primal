using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using InvestmentPortfolioTracker.Api.AssetItems;
using InvestmentPortfolioTracker.Api.Transactions;

namespace InvestmentPortfolioTracker.E2ETests;

internal static class HttpClientExtensions
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() },
	};

	internal static async Task<AssetItemResponse> AddAssetItemAsync(
		this HttpClient client, string name, string assetClass, string assetType, string externalId, string currency)
	{
		var response = await client.PostAsJsonAsync("/api/asset-items", new
		{
			Name = name,
			AssetClass = assetClass,
			AssetType = assetType,
			ExternalId = externalId,
			Currency = currency,
		});

		return await response.ReadJsonAsync<AssetItemResponse>();
	}

	internal static async Task<TransactionResponse> AddTransactionAsync(
		this HttpClient client,
		Guid assetItemId,
		string date,
		string name,
		string transactionType,
		decimal units,
		decimal price,
		decimal amount)
	{
		var response = await client.PostAsJsonAsync(
			$"/api/asset-items/{assetItemId}/transactions", new
			{
				AssetItemId = assetItemId,
				Date = date,
				Name = name,
				TransactionType = transactionType,
				Units = units,
				Price = price,
				Amount = amount,
			});

		return await response.ReadJsonAsync<TransactionResponse>();
	}

	private static async Task<T> ReadJsonAsync<T>(this HttpResponseMessage response)
	{
		response.EnsureSuccessStatusCode();
		var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
		return result!;
	}
}
