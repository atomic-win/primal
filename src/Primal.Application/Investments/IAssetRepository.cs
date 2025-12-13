using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Application.Investments;

public interface IAssetRepository
{
	Task<Asset> GetByIdAsync(
		AssetId assetId,
		CancellationToken cancellationToken);

	Task<Asset> GetByExternalIdAsync(
		string externalId,
		CancellationToken cancellationToken);

	Task<Asset> AddAsync(
		string name,
		AssetClass assetClass,
		AssetType assetType,
		Currency currency,
		string externalId,
		CancellationToken cancellationToken);
}
