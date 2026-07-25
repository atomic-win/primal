using System.Collections.Immutable;

using Microsoft.Extensions.Caching.Hybrid;

using InvestmentPortfolioTracker.Core.Investments;
using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Infrastructure.Investments;

internal sealed class CachedTransactionRepository : ITransactionRepository
{
	private readonly HybridCache cache;
	private readonly TimeProvider timeProvider;
	private readonly ITransactionRepository transactionRepository;

	internal CachedTransactionRepository(
		HybridCache cache,
		TimeProvider timeProvider,
		ITransactionRepository transactionRepository)
	{
		this.cache = cache;
		this.timeProvider = timeProvider;
		this.transactionRepository = transactionRepository;
	}

	public async Task<IEnumerable<Transaction>> GetByAssetItemIdAsync(
		UserId userId,
		AssetItemId assetItemId,
		CancellationToken cancellationToken)
	{
		return await this.cache.GetOrCreateAsync(
			userId.TransactionsKey(assetItemId),
			async entry => (await this.transactionRepository.GetByAssetItemIdAsync(
				userId,
				assetItemId,
				cancellationToken)).ToImmutableArray(),
			tags: new[] { userId.TransactionsKey(assetItemId) },
			cancellationToken: cancellationToken);
	}

	public async Task<Transaction> GetByIdAsync(
		UserId userId,
		AssetItemId assetItemId,
		TransactionId transactionId,
		CancellationToken cancellationToken)
	{
		return await this.cache.GetOrCreateAsync(
			userId.TransactionKey(assetItemId, transactionId),
			async entry => await this.transactionRepository.GetByIdAsync(
				userId,
				assetItemId,
				transactionId,
				cancellationToken),
			tags: new[] { userId.TransactionsKey(assetItemId) },
			cancellationToken: cancellationToken);
	}

	public async Task<Transaction> AddAsync(
		UserId userId,
		AssetItemId assetItemId,
		DateOnly date,
		string name,
		TransactionType type,
		decimal units,
		decimal price,
		decimal amount,
		CancellationToken cancellationToken)
	{
		var transaction = await this.transactionRepository.AddAsync(
			userId,
			assetItemId,
			date,
			name,
			type,
			units,
			price,
			amount,
			cancellationToken);

		await this.InvalidateCacheAsync(
			userId,
			assetItemId,
			date,
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
			transaction.Date,
			cancellationToken);
	}

	public async Task DeleteAsync(
		UserId userId,
		AssetItemId assetItemId,
		TransactionId transactionId,
		CancellationToken cancellationToken)
	{
		var transaction = await this.GetByIdAsync(
			userId,
			assetItemId,
			transactionId,
			cancellationToken);

		await this.transactionRepository.DeleteAsync(
			userId,
			assetItemId,
			transactionId,
			cancellationToken);

		await this.InvalidateCacheAsync(
			userId,
			assetItemId,
			transaction.Date,
			cancellationToken);
	}

	private async Task InvalidateCacheAsync(
		UserId userId,
		AssetItemId assetItemId,
		DateOnly transactionDate,
		CancellationToken cancellationToken)
	{
		await this.cache.RemoveByTagAsync(
			userId.TransactionsKey(assetItemId),
			cancellationToken: cancellationToken);

		var valuationDates = this.timeProvider.GetValuationDates(transactionDate);

		await Task.WhenAll(valuationDates.Select(valuationDate =>
			this.cache.RemoveByTagAsync(
				userId.ValuationTag(assetItemId, valuationDate),
				cancellationToken: cancellationToken).AsTask()));
	}
}
