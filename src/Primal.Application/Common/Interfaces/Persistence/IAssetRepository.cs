using ErrorOr;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Application.Common.Interfaces.Persistence;

public interface IAssetRepository
{
	Task<ErrorOr<IEnumerable<Asset>>> GetAllAsync(UserId userId, CancellationToken cancellationToken);

	Task<ErrorOr<Asset>> GetByIdAsync(UserId userId, AssetId assetId, CancellationToken cancellationToken);

	Task<ErrorOr<Asset>> AddAsync(UserId userId, string name, InstrumentId instrumentId, CancellationToken cancellationToken);

	Task<ErrorOr<Success>> DeleteAsync(UserId userId, AssetId assetId, CancellationToken cancellationToken);
}
