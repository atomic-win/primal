using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Core.Investments;

public interface ITransactionRepository
{
	Task<IEnumerable<Transaction>> GetByAssetItemIdAsync(
		UserId userId,
		AssetItemId assetItemId,
		CancellationToken cancellationToken);

	Task<Transaction> GetByIdAsync(
		UserId userId,
		AssetItemId assetItemId,
		TransactionId transactionId,
		CancellationToken cancellationToken);

	Task<Transaction> AddAsync(
		UserId userId,
		AssetItemId assetItemId,
		DateOnly date,
		string name,
		TransactionType type,
		decimal units,
		decimal price,
		decimal amount,
		CancellationToken cancellationToken);

	Task UpdateAsync(
		UserId userId,
		Transaction transaction,
		CancellationToken cancellationToken);

	Task DeleteAsync(
		UserId userId,
		AssetItemId assetItemId,
		TransactionId transactionId,
		CancellationToken cancellationToken);
}
