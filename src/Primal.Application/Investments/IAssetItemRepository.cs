using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public interface IAssetItemRepository
{
	Task<IEnumerable<AssetItem>> GetAllAsync(
		UserId userId,
		CancellationToken cancellationToken);

	Task<AssetItem> AddAsync(
		UserId userId,
		AssetId assetId,
		string name,
		CancellationToken cancellationToken);
}
