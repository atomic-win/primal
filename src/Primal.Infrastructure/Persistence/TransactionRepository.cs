using System.Globalization;
using Azure;
using Azure.Data.Tables;
using ErrorOr;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Persistence;

internal sealed class TransactionRepository : ITransactionRepository
{
	private readonly TableClient transactionTableClient;

	internal TransactionRepository(TableClient transactionTableClient)
	{
		this.transactionTableClient = transactionTableClient;
	}

	public async Task<ErrorOr<IEnumerable<Transaction>>> GetAllAsync(
		UserId userId,
		CancellationToken cancellationToken)
	{
		AsyncPageable<TransactionTableEntity> entities = this.transactionTableClient.QueryAsync<TransactionTableEntity>(
			entity => entity.PartitionKey == userId.Value.ToString("N"),
			cancellationToken: cancellationToken);

		List<Transaction> transactions = new List<Transaction>();

		await foreach (var entity in entities.WithCancellation(cancellationToken))
		{
			transactions.Add(this.MapToTransaction(entity));
		}

		return transactions;
	}

	public async Task<ErrorOr<Transaction>> GetByIdAsync(
		UserId userId,
		TransactionId transactionId,
		CancellationToken cancellationToken)
	{
		try
		{
			TransactionTableEntity entity = await this.transactionTableClient.GetEntityAsync<TransactionTableEntity>(
				userId.Value.ToString("N"),
				transactionId.Value.ToString("N"),
				cancellationToken: cancellationToken);

			return this.MapToTransaction(entity);
		}
		catch (RequestFailedException ex) when (ex.Status == 404)
		{
			return Error.NotFound();
		}
		catch (Exception ex)
		{
			return Error.Failure(ex.Message);
		}
	}

	public async Task<ErrorOr<Transaction>> AddAsync(
		UserId userId,
		DateOnly date,
		string name,
		TransactionType type,
		AssetId assetId,
		decimal units,
		CancellationToken cancellationToken)
	{
		var transaction = new Transaction(
			new TransactionId(Guid.NewGuid()),
			date,
			name,
			type,
			assetId,
			units);

		try
		{
			TransactionTableEntity entity = new TransactionTableEntity
			{
				PartitionKey = userId.Value.ToString("N"),
				RowKey = transaction.Id.Value.ToString("N"),
				Date = date.ToString(CultureInfo.InvariantCulture),
				Name = name,
				Type = type.ToString(),
				AssetId = assetId.Value.ToString("N"),
				Units = units.ToString(CultureInfo.InvariantCulture),
			};

			await this.transactionTableClient.AddEntityAsync(entity, cancellationToken: cancellationToken);

			return transaction;
		}
		catch (Exception ex)
		{
			return Error.Failure(ex.Message);
		}
	}

	public async Task<ErrorOr<Success>> DeleteAsync(UserId userId, TransactionId transactionId, CancellationToken cancellationToken)
	{
		try
		{
			await this.transactionTableClient.DeleteEntityAsync(
				partitionKey: userId.Value.ToString("N"),
				rowKey: transactionId.Value.ToString("N"),
				cancellationToken: cancellationToken);

			return Result.Success;
		}
		catch (RequestFailedException ex) when (ex.Status == 404)
		{
			return Error.NotFound();
		}
		catch (Exception ex)
		{
			return Error.Failure(description: ex.Message);
		}
	}

	private Transaction MapToTransaction(TransactionTableEntity entity)
	{
		string units = string.IsNullOrWhiteSpace(entity.Units) ? entity.Amount : entity.Units;

		return new Transaction(
			new TransactionId(Guid.Parse(entity.RowKey)),
			DateOnly.Parse(entity.Date, CultureInfo.InvariantCulture),
			entity.Name,
			Enum.Parse<TransactionType>(entity.Type),
			new AssetId(Guid.Parse(entity.AssetId)),
			decimal.Parse(units, CultureInfo.InvariantCulture));
	}

	private sealed class TransactionTableEntity : ITableEntity
	{
		public string PartitionKey { get; set; }

		public string RowKey { get; set; }

		public DateTimeOffset? Timestamp { get; set; }

		public ETag ETag { get; set; }

		public string Date { get; set; }

		public string Name { get; set; }

		public string Type { get; set; }

		public string AssetId { get; set; }

		public string Units { get; set; }

		public string Amount { get; set; }
	}
}
