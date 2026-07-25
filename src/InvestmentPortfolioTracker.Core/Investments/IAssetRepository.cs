using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Money;

namespace InvestmentPortfolioTracker.Core.Investments;

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
