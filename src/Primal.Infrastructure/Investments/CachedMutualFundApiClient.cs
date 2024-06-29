using ErrorOr;
using Primal.Application.Common.Interfaces;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Domain.Investments;
using Primal.Infrastructure.Common;

namespace Primal.Infrastructure.Investments;

internal sealed class CachedMutualFundApiClient : IMutualFundApiClient
{
	private readonly ICache cache;
	private readonly IMutualFundApiClient mutualFundApiClient;

	public CachedMutualFundApiClient(
		ICache cache,
		IMutualFundApiClient mutualFundApiClient)
	{
		this.cache = cache;
		this.mutualFundApiClient = mutualFundApiClient;
	}

	public async Task<ErrorOr<MutualFund>> GetBySchemeCodeAsync(int schemeCode, CancellationToken cancellationToken)
	{
		return await this.cache.GetOrCreateAsync(
			$"mutualfund/{schemeCode}",
			async (cancelToken) => await this.mutualFundApiClient.GetBySchemeCodeAsync(schemeCode, cancelToken),
			TimeSpan.FromHours(1),
			cancellationToken);
	}

	public async Task<ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>> GetPriceAsync(int schemeCode, CancellationToken cancellationToken)
	{
		return await this.cache.GetOrCreateAsync(
			$"mutualfunds/{schemeCode}/price",
			async (cancelToken) => await this.mutualFundApiClient.GetPriceAsync(schemeCode, cancelToken),
			TimeSpan.FromHours(1),
			cancellationToken);
	}
}
