using System.Text.Json;
using ErrorOr;
using Microsoft.Extensions.Caching.Distributed;

namespace Primal.Infrastructure.Common;

internal static class DistributedCacheExtensions
{
	internal static async Task<ErrorOr<T>> GetOrCreateAsync<T>(
		this IDistributedCache cache,
		string key,
		Func<CancellationToken, Task<ErrorOr<T>>> createFn,
		TimeSpan relativeExpiration,
		CancellationToken cancellationToken)
	{
		var cachedJson = await cache.GetStringAsync(key, cancellationToken);

		if (!string.IsNullOrEmpty(cachedJson))
		{
			return JsonSerializer.Deserialize<T>(cachedJson).ToErrorOr();
		}

		var errorOrValue = await createFn(cancellationToken);

		if (!errorOrValue.IsError)
		{
			await cache.SetStringAsync(
				key,
				JsonSerializer.Serialize(errorOrValue.Value),
				new DistributedCacheEntryOptions
				{
					AbsoluteExpirationRelativeToNow = relativeExpiration,
				},
				cancellationToken);
		}

		return errorOrValue;
	}
}
