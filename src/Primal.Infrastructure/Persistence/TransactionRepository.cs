using System.Collections.Immutable;
using System.Globalization;
using ErrorOr;
using LiteDB;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Persistence;

internal sealed class TransactionRepository : ITransactionRepository
{
	private readonly LiteDatabase liteDatabase;

	internal TransactionRepository(LiteDatabase liteDatabase)
	{
		this.liteDatabase = liteDatabase;

		var collection = this.liteDatabase.GetCollection<TransactionTableEntity>("Transactions");
		collection.EnsureIndex(x => x.Id, unique: true);
		collection.EnsureIndex(x => x.UserId);
		collection.EnsureIndex(x => x.AssetId);
	}

	public async Task<ErrorOr<Transaction>> GetByIdAsync(
		UserId userId,
		AssetId assetId,
		TransactionId transactionId,
		CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var collection = this.liteDatabase.GetCollection<TransactionTableEntity>("Transactions");

		var transactionTableEntity = collection
			.FindOne(x => x.Id == transactionId.Value && x.UserId == userId.Value && x.AssetId == assetId.Value);

		return transactionTableEntity != null
			? this.MapToTransaction(transactionTableEntity)
			: Error.NotFound(description: "Transaction not found.");
	}

	public async Task<ErrorOr<IEnumerable<Transaction>>> GetByAssetIdAsync(
		UserId userId,
		AssetId assetId,
		CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var collection = this.liteDatabase.GetCollection<TransactionTableEntity>("Transactions");

		return collection
			.Find(x => x.UserId == userId.Value && x.AssetId == assetId.Value)
			.Select(this.MapToTransaction)
			.ToImmutableArray();
	}

	public async Task<ErrorOr<Transaction>> AddAsync(
		UserId userId,
		AssetId assetId,
		DateOnly date,
		string name,
		TransactionType type,
		decimal units,
		CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var collection = this.liteDatabase.GetCollection<TransactionTableEntity>("Transactions");

		var transactionTableEntity = new TransactionTableEntity
		{
			Id = Ulid.NewUlid(new DateTimeOffset(date, TimeOnly.MinValue, TimeSpan.Zero)).ToGuid(),
			UserId = userId.Value,
			AssetId = assetId.Value,
			Date = date.ToString(CultureInfo.InvariantCulture),
			Name = name,
			Type = type,
			Units = units,
		};

		collection.Insert(transactionTableEntity);

		return this.MapToTransaction(transactionTableEntity);
	}

	public async Task<ErrorOr<Success>> DeleteAsync(
		UserId userId,
		AssetId assetId,
		TransactionId transactionId,
		CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var collection = this.liteDatabase.GetCollection<TransactionTableEntity>("Transactions");

		var transactionTableEntity = collection
			.FindOne(x => x.Id == transactionId.Value && x.UserId == userId.Value && x.AssetId == assetId.Value);

		if (transactionTableEntity == null)
		{
			return Error.NotFound(description: "Transaction not found.");
		}

		collection.Delete(transactionTableEntity.Id);

		return Result.Success;
	}

	private Transaction MapToTransaction(TransactionTableEntity transactionTableEntity)
	{
		return new Transaction(
			new TransactionId(transactionTableEntity.Id),
			DateOnly.Parse(transactionTableEntity.Date, CultureInfo.InvariantCulture),
			transactionTableEntity.Name,
			transactionTableEntity.Type,
			new AssetId(transactionTableEntity.AssetId),
			transactionTableEntity.Units);
	}

	private sealed class TransactionTableEntity
	{
		public Guid Id { get; set; }

		public Guid UserId { get; set; }

		public Guid AssetId { get; set; }

		public string Date { get; set; }

		public string Name { get; set; }

		public TransactionType Type { get; set; }

		public decimal Units { get; set; }
	}
}
