using ErrorOr;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Application.Common.Interfaces.Persistence;

public interface ITransactionRepository
{
	Task<ErrorOr<Transaction>> GetByIdAsync(
		UserId userId,
		AssetId assetId,
		TransactionId transactionId,
		CancellationToken cancellationToken);

	Task<ErrorOr<IEnumerable<Transaction>>> GetByAssetIdAsync(
		UserId userId,
		AssetId assetId,
		CancellationToken cancellationToken);

	Task<ErrorOr<Transaction>> AddAsync(
		UserId userId,
		AssetId assetId,
		DateOnly date,
		string name,
		TransactionType type,
		decimal units,
		CancellationToken cancellationToken);

	Task<ErrorOr<Success>> DeleteAsync(
		UserId userId,
		AssetId assetId,
		TransactionId transactionId,
		CancellationToken cancellationToken);
}
