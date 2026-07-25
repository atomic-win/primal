using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Core.Investments;

public interface IAssetItemRepository
{
	Task<IEnumerable<AssetItem>> GetAllAsync(
		UserId userId,
		CancellationToken cancellationToken);

	Task<AssetItem> GetByIdAsync(
		UserId userId,
		AssetItemId assetItemId,
		CancellationToken cancellationToken);

	Task<AssetItem> AddAsync(
		UserId userId,
		AssetId assetId,
		string name,
		CancellationToken cancellationToken);

	Task UpdateAsync(
		UserId userId,
		AssetItem assetItem,
		CancellationToken cancellationToken);

	Task DeleteAsync(
		UserId userId,
		AssetItemId assetItemId,
		CancellationToken cancellationToken);
}
