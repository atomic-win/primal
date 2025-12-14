using Microsoft.EntityFrameworkCore;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;
using Primal.Infrastructure.Persistence;

namespace Primal.Infrastructure.Investments;

internal sealed class TransactionRepository : ITransactionRepository
{
	private readonly AppDbContext appDbContext;

	internal TransactionRepository(AppDbContext appDbContext)
	{
		this.appDbContext = appDbContext;
	}

	public async Task<IEnumerable<Transaction>> GetByAssetItemIdAsync(
		UserId userId,
		AssetItemId assetItemId,
		DateOnly maxDate,
		CancellationToken cancellationToken)
	{
		var transactionTableEntities = await this.appDbContext.Transactions
			.Where(t => t.UserId == userId.Value && t.AssetItemId == assetItemId.Value && t.Date <= maxDate)
			.ToListAsync(cancellationToken);

		return transactionTableEntities
			.AsParallel()
			.Select(this.MapToTransaction);
	}

	public async Task<Transaction> GetByIdAsync(
		UserId userId,
		AssetItemId assetItemId,
		TransactionId transactionId,
		CancellationToken cancellationToken)
	{
		var transactionTableEntity = await this.appDbContext.Transactions
			.FirstOrDefaultAsync(
				t =>
				t.UserId == userId.Value &&
				t.AssetItemId == assetItemId.Value &&
				t.Id == transactionId.Value,
				cancellationToken);

		if (transactionTableEntity is null)
		{
			return Transaction.Empty;
		}

		return this.MapToTransaction(transactionTableEntity);
	}

	public async Task<DateOnly> GetEarliestTransactionDateAsync(
		UserId userId,
		AssetItemId assetItemId,
		CancellationToken cancellationToken)
	{
		var earliestDate = await this.appDbContext.Transactions
			.Where(t => t.UserId == userId.Value && t.AssetItemId == assetItemId.Value)
			.MinAsync(t => (DateOnly?)t.Date, cancellationToken);

		return earliestDate ?? default;
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
		var transactionTableEntity = new TransactionTableEntity
		{
			Id = Guid.CreateVersion7(),
			UserId = userId.Value,
			AssetItemId = assetItemId.Value,
			Date = date,
			Name = name,
			TransactionType = type,
			Units = units,
		};

		await this.appDbContext.Transactions.AddAsync(transactionTableEntity, cancellationToken);

		return this.MapToTransaction(transactionTableEntity);
	}

	public async Task UpdateAsync(
		UserId userId,
		Transaction transaction,
		CancellationToken cancellationToken)
	{
		await this.appDbContext.Transactions
			.Where(t => t.UserId == userId.Value && t.Id == transaction.Id.Value)
			.ExecuteUpdateAsync(
				s => s
					.SetProperty(t => t.Date, transaction.Date)
					.SetProperty(t => t.Name, transaction.Name)
					.SetProperty(t => t.TransactionType, transaction.TransactionType)
					.SetProperty(t => t.Units, transaction.Units),
				cancellationToken);
	}

	public async Task DeleteAsync(
		UserId userId,
		AssetItemId assetItemId,
		TransactionId transactionId,
		CancellationToken cancellationToken)
	{
		await this.appDbContext.Transactions
			.Where(t =>
				t.UserId == userId.Value &&
				t.AssetItemId == assetItemId.Value &&
				t.Id == transactionId.Value)
			.ExecuteDeleteAsync(cancellationToken);
	}

	private Transaction MapToTransaction(TransactionTableEntity entity)
	{
		return new Transaction(
			new TransactionId(entity.Id),
			entity.Date,
			entity.Name,
			entity.TransactionType,
			new AssetItemId(entity.AssetItemId),
			entity.Units);
	}
}
