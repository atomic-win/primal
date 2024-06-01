using System.Text.Json;
using ErrorOr;
using Microsoft.Extensions.Caching.Distributed;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Domain.Money;
using Primal.Infrastructure.Common;

namespace Primal.Infrastructure.Investments;

internal sealed class CachedExchangeRateProvider : IExchangeRateProvider
{
	private readonly IDistributedCache cache;
	private readonly IExchangeRateProvider exchangeRateProvider;

	public CachedExchangeRateProvider(
		IDistributedCache cache,
		IExchangeRateProvider exchangeRateProvider)
	{
		this.cache = cache;
		this.exchangeRateProvider = exchangeRateProvider;
	}

	public async Task<ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>> GetExchangeRatesAsync(
		Currency from,
		Currency to,
		CancellationToken cancellationToken)
	{
		return await this.cache.GetOrCreateAsync(
			$"ExchangeRates_{from}_{to}",
			async (cancelToken) => await this.exchangeRateProvider.GetExchangeRatesAsync(from, to, cancelToken),
			TimeSpan.FromDays(1),
			cancellationToken);
	}
}
