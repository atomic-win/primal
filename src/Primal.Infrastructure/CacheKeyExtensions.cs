using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Infrastructure;

public static class CacheKeyExtensions
{
	// Valuations (public - used by Api project)
	public static string ValuationKey(this UserId userId, IReadOnlyList<AssetItemId> assetItemIds, DateOnly valuationDate, Currency currency)
	{
		var assetItemIdsHash = assetItemIds.Order()
			.Aggregate(0, (hash, id) => HashCode.Combine(hash, id.GetHashCode()));

		return $"users/{userId.Value}/asset-items/valuations/{valuationDate:yyyy-MM-dd}/{currency}/{assetItemIdsHash}";
	}

	public static string ValuationTag(this UserId userId, AssetItemId assetItemId, DateOnly valuationDate)
		=> $"users/{userId.Value}/asset-items/{assetItemId.Value}/valuations/{valuationDate:yyyy-MM-dd}";

	public static string ValuationInputKey(this UserId userId, AssetItemId assetItemId, DateOnly valuationDate, Currency currency)
		=> $"users/{userId.Value}/asset-items/{assetItemId.Value}/valuation-inputs/{valuationDate:yyyy-MM-dd}/{currency}";

	// User
	internal static string UserKey(this UserId userId)
		=> $"users/{userId.Value}";

	// Asset Items
	internal static string AssetItemsKey(this UserId userId)
		=> $"users/{userId.Value}/asset-items";

	internal static string AssetItemKey(this UserId userId, AssetItemId assetItemId)
		=> $"users/{userId.Value}/asset-items/{assetItemId.Value}";

	// Transactions
	internal static string TransactionsKey(this UserId userId, AssetItemId assetItemId)
		=> $"users/{userId.Value}/asset-items/{assetItemId.Value}/transactions";

	internal static string TransactionKey(this UserId userId, AssetItemId assetItemId, TransactionId transactionId)
		=> $"users/{userId.Value}/asset-items/{assetItemId.Value}/transactions/{transactionId.Value}";

	// Assets
	internal static string AssetKey(this AssetId assetId)
		=> $"assets/{assetId.Value}";

	internal static string AssetByExternalIdKey(string externalId)
		=> $"assets/external/{externalId}";

	// Asset API
	internal static string AssetApiKey<T>(string symbol)
		=> $"asset-api/{typeof(T).Name}/{symbol}";

	internal static string AssetApiPricesKey<T>(string symbol)
		=> $"asset-api/{typeof(T).Name}/{symbol}/prices";

	internal static string AssetApiOnOrBeforePriceKey<T>(string symbol, DateOnly date)
		=> $"asset-api/{typeof(T).Name}/{symbol}/prices/{date:yyyy-MM-dd}/on-or-before";

	// Forex
	internal static string ForexRatesKey(Currency fromCurrency, Currency toCurrency)
		=> $"forex/{fromCurrency}-{toCurrency}/rates";

	internal static string ForexOnOrBeforeRateKey(Currency fromCurrency, Currency toCurrency, DateOnly date)
		=> $"forex/{fromCurrency}-{toCurrency}/rates/{date:yyyy-MM-dd}/on-or-before";
}
