using System.Net.Http.Json;
using Primal.Api.AssetItems;
using Primal.Api.Transactions;

namespace Primal.E2ETests;

internal static class HttpClientExtensions
{
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
}
