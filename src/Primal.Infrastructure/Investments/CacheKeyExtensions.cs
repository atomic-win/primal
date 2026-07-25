using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Investments;

public static class CacheKeyExtensions
{
	public static string ValuationKey(this UserId userId, IReadOnlyList<AssetItemId> assetItemIds, DateOnly valuationDate, Currency currency)
	{
		var assetItemIdsHash = assetItemIds.Order()
			.Aggregate(0, (hash, id) => HashCode.Combine(hash, id.GetHashCode()));

		return $"users/{userId.Value}/asset-items/valuations?date={valuationDate:yyyy-MM-dd}&currency={currency}&assetItemIdsHash={assetItemIdsHash}";
	}

	public static string ValuationTag(this UserId userId, AssetItemId assetItemId, DateOnly valuationDate)
		=> $"users/{userId.Value}/asset-items/{assetItemId.Value}/valuations?date={valuationDate:yyyy-MM-dd}";

	internal static string AssetItemsKey(this UserId userId)
		=> $"users/{userId.Value}/assetItems";

	internal static string AssetItemKey(this UserId userId, AssetItemId assetItemId)
		=> $"users/{userId.Value}/assetItems/{assetItemId.Value}";

	internal static string TransactionsKey(this UserId userId, AssetItemId assetItemId)
		=> $"users/{userId.Value}/assetItems/{assetItemId.Value}/transactions";

	internal static string TransactionKey(this UserId userId, AssetItemId assetItemId, TransactionId transactionId)
		=> $"users/{userId.Value}/assetItems/{assetItemId.Value}/transactions/{transactionId.Value}";
}
