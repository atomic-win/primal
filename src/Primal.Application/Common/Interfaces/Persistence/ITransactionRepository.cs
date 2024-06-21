using ErrorOr;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Application.Common.Interfaces.Persistence;

public interface ITransactionRepository
{
	Task<ErrorOr<IEnumerable<Transaction>>> GetAllAsync(
		UserId userId,
		CancellationToken cancellationToken);

	Task<ErrorOr<Transaction>> GetByIdAsync(
		UserId userId,
		TransactionId transactionId,
		CancellationToken cancellationToken);

	Task<ErrorOr<Transaction>> AddAsync(
		UserId userId,
		DateOnly date,
		string name,
		TransactionType type,
		AssetId assetId,
		decimal units,
		CancellationToken cancellationToken);

	Task<ErrorOr<Success>> DeleteAsync(
		UserId userId,
		TransactionId transactionId,
		CancellationToken cancellationToken);
}
