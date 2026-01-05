using Microsoft.Extensions.Caching.Hybrid;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Investments;

internal sealed class CachedTransactionRepository : ITransactionRepository
{
	private readonly HybridCache hybridCache;
	private readonly ITransactionRepository transactionRepository;

	internal CachedTransactionRepository(
		HybridCache hybridCache,
		ITransactionRepository transactionRepository)
	{
		this.hybridCache = hybridCache;
		this.transactionRepository = transactionRepository;
	}

	public async Task<IEnumerable<Transaction>> GetByAssetItemIdAsync(
		UserId userId,
		AssetItemId assetItemId,
		CancellationToken cancellationToken)
	{
		return await this.hybridCache.GetOrCreateAsync(
			$"users/{userId.Value}/assetItems/{assetItemId.Value}/transactions",
			async entry => await this.transactionRepository.GetByAssetItemIdAsync(
				userId,
				assetItemId,
				cancellationToken),
			tags: new[] { $"users/{userId.Value}/assetItems/{assetItemId.Value}/transactions" },
			cancellationToken: cancellationToken);
	}

	public async Task<Transaction> GetByIdAsync(
		UserId userId,
		AssetItemId assetItemId,
		TransactionId transactionId,
		CancellationToken cancellationToken)
	{
		return await this.hybridCache.GetOrCreateAsync(
			$"users/{userId.Value}/assetItems/{assetItemId.Value}/transactions/{transactionId.Value}",
			async entry => await this.transactionRepository.GetByIdAsync(
				userId,
				assetItemId,
				transactionId,
				cancellationToken),
			cancellationToken: cancellationToken);
	}

	public async Task<DateOnly> GetEarliestTransactionDateAsync(
		UserId userId,
		AssetItemId assetItemId,
		CancellationToken cancellationToken)
	{
		return await this.hybridCache.GetOrCreateAsync(
			$"users/{userId.Value}/assetItems/{assetItemId.Value}/transactions/earliestDate",
			async entry => await this.transactionRepository.GetEarliestTransactionDateAsync(
				userId,
				assetItemId,
				cancellationToken),
			tags: new[] { $"users/{userId.Value}/assetItems/{assetItemId.Value}/transactions" },
			cancellationToken: cancellationToken);
	}

	public async Task<Transaction> AddAsync(
		UserId userId,
		AssetItemId assetItemId,
		DateOnly date,
		string name,
		TransactionType type,
		decimal units,
		CancellationToken cancellationToken)
	{
		var transaction = await this.transactionRepository.AddAsync(
			userId,
			assetItemId,
			date,
			name,
			type,
			units,
			cancellationToken);

		await this.InvalidateCacheAsync(
			userId,
			assetItemId,
			cancellationToken);

		return transaction;
	}

	public async Task UpdateAsync(
		UserId userId,
		Transaction transaction,
		CancellationToken cancellationToken)
	{
		await this.transactionRepository.UpdateAsync(
			userId,
			transaction,
			cancellationToken);

		await this.InvalidateCacheAsync(
			userId,
			transaction.AssetItemId,
			cancellationToken);
	}

	public async Task DeleteAsync(
		UserId userId,
		AssetItemId assetItemId,
		TransactionId transactionId,
		CancellationToken cancellationToken)
	{
		await this.transactionRepository.DeleteAsync(
			userId,
			assetItemId,
			transactionId,
			cancellationToken);

		await this.InvalidateCacheAsync(
			userId,
			assetItemId,
			cancellationToken);
	}

	private async Task InvalidateCacheAsync(
		UserId userId,
		AssetItemId assetItemId,
		CancellationToken cancellationToken)
	{
		await this.hybridCache.RemoveByTagAsync(
			$"users/{userId.Value}/assetItems/{assetItemId.Value}/transactions",
			cancellationToken: cancellationToken);

		await this.hybridCache.RemoveByTagAsync(
			$"users/{userId.Value}/assetItems/{assetItemId.Value}/valuation",
			cancellationToken: cancellationToken);
	}
}
