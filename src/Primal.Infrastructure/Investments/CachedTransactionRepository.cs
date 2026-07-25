using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Hybrid;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Investments;

internal sealed class CachedTransactionRepository : ITransactionRepository
{
	private readonly HybridCache hybridCache;
	private readonly TimeProvider timeProvider;
	private readonly ITransactionRepository transactionRepository;

	internal CachedTransactionRepository(
		HybridCache hybridCache,
		TimeProvider timeProvider,
		ITransactionRepository transactionRepository)
	{
		this.hybridCache = hybridCache;
		this.timeProvider = timeProvider;
		this.transactionRepository = transactionRepository;
	}

	public async Task<IEnumerable<Transaction>> GetByAssetItemIdAsync(
		UserId userId,
		AssetItemId assetItemId,
		CancellationToken cancellationToken)
	{
		return await this.hybridCache.GetOrCreateAsync(
			$"users/{userId.Value}/assetItems/{assetItemId.Value}/transactions",
			async entry => (await this.transactionRepository.GetByAssetItemIdAsync(
				userId,
				assetItemId,
				cancellationToken)).ToImmutableArray(),
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
		await this.hybridCache.RemoveByTagAsync(
			$"users/{userId.Value}/assetItems/{assetItemId.Value}/transactions",
			cancellationToken: cancellationToken);

		var valuationDates = this.timeProvider.GetValuationDates(transactionDate);

		await Task.WhenAll(valuationDates.Select(valuationDate =>
			this.hybridCache.RemoveByTagAsync(
				$"users/{userId.Value}/asset-items/{assetItemId.Value}/valuations?date={valuationDate:yyyy-MM-dd}",
				cancellationToken: cancellationToken).AsTask()));
	}
}
