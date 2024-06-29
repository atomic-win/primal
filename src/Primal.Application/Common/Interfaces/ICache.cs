using ErrorOr;

namespace Primal.Application.Common.Interfaces;

public interface ICache
{
	Task<ErrorOr<T>> GetAsync<T>(
		string key,
		CancellationToken cancellationToken);

	Task<ErrorOr<Success>> SetAsync<T>(
		string key,
		T value,
		TimeSpan relativeExpiration,
		CancellationToken cancellationToken);
}
