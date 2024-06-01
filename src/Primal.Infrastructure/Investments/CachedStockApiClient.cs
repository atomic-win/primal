using ErrorOr;
using Microsoft.Extensions.Caching.Distributed;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Domain.Investments;
using Primal.Infrastructure.Common;

namespace Primal.Infrastructure.Investments;

internal sealed class CachedStockApiClient : IStockApiClient
{
	private readonly IDistributedCache cache;
	private readonly IStockApiClient stockApiClient;

	public CachedStockApiClient(IDistributedCache cache, IStockApiClient stockApiClient)
	{
		this.cache = cache;
		this.stockApiClient = stockApiClient;
	}

	public async Task<ErrorOr<Stock>> GetBySymbolAsync(string symbol, CancellationToken cancellationToken)
	{
		return await this.cache.GetOrCreateAsync(
			$"Stock_{symbol}",
			async (cancelToken) => await this.stockApiClient.GetBySymbolAsync(symbol, cancelToken),
			TimeSpan.FromDays(1),
			cancellationToken);
	}

	public async Task<ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>> GetHistoricalValuesAsync(string symbol, CancellationToken cancellationToken)
	{
		return await this.cache.GetOrCreateAsync(
			$"Stock_{symbol}_HistoricalValues",
			async (cancelToken) => await this.stockApiClient.GetHistoricalValuesAsync(symbol, cancelToken),
			TimeSpan.FromDays(1),
			cancellationToken);
	}
}
