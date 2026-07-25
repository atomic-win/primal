using System.Globalization;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Investments;

public static class CacheKeyExtensions
{
	public static string ValuationTag(this UserId userId, AssetItemId assetItemId, DateOnly valuationDate)
		=> string.Create(CultureInfo.InvariantCulture, $"users/{userId.Value}/asset-items/{assetItemId.Value}/valuations?date={valuationDate:yyyy-MM-dd}");

	internal static string AssetItemsKey(this UserId userId)
		=> $"users/{userId.Value}/assetItems";

	internal static string AssetItemKey(this UserId userId, AssetItemId assetItemId)
		=> $"users/{userId.Value}/assetItems/{assetItemId.Value}";

	internal static string TransactionsKey(this UserId userId, AssetItemId assetItemId)
		=> $"users/{userId.Value}/assetItems/{assetItemId.Value}/transactions";

	internal static string TransactionKey(this UserId userId, AssetItemId assetItemId, TransactionId transactionId)
		=> $"users/{userId.Value}/assetItems/{assetItemId.Value}/transactions/{transactionId.Value}";
}
