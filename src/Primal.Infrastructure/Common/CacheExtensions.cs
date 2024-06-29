using ErrorOr;
using Primal.Application.Common.Interfaces;

namespace Primal.Infrastructure.Common;

internal static class CacheExtensions
{
	internal static async Task<ErrorOr<T>> GetOrCreateAsync<T>(
		this ICache cache,
		string key,
		Func<CancellationToken, Task<ErrorOr<T>>> createFn,
		TimeSpan relativeExpiration,
		CancellationToken cancellationToken)
	{
		var errorOrValue = await cache.GetAsync<T>(key, cancellationToken);

		if (!errorOrValue.IsError)
		{
			return errorOrValue;
		}

		errorOrValue = await createFn(cancellationToken);

		if (!errorOrValue.IsError)
		{
			await cache.SetAsync(key, errorOrValue.Value, relativeExpiration, cancellationToken);
		}

		return errorOrValue;
	}
}
