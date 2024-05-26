using System.Globalization;
using Azure;
using Azure.Data.Tables;
using ErrorOr;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;
using Primal.Domain.Money;
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
		AsyncPageable<TableEntity> entities = this.transactionTableClient.QueryAsync<TableEntity>(
			entity => entity.PartitionKey == userId.Value.ToString("N"),
			cancellationToken: cancellationToken);

		List<Transaction> transactions = new List<Transaction>();

		await foreach (TableEntity entity in entities.WithCancellation(cancellationToken))
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
			TableEntity entity = await this.transactionTableClient.GetEntityAsync<TableEntity>(
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

	public async Task<ErrorOr<Transaction>> AddBuySellTransactionAsync(
		UserId userId,
		DateOnly date,
		string name,
		TransactionType type,
		AssetId assetId,
		decimal units,
		CancellationToken cancellationToken)
	{
		var transaction = new BuySellTransaction(
			new TransactionId(Guid.NewGuid()),
			date,
			name,
			type,
			assetId,
			units);

		try
		{
			TransactionTableEntity entity = new BuySellTransactionTableEntity
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

	public async Task<ErrorOr<Transaction>> AddCashTransactionAsync(
		UserId userId,
		DateOnly date,
		string name,
		TransactionType type,
		AssetId assetId,
		decimal amount,
		Currency currency,
		CancellationToken cancellationToken)
	{
		var transaction = new CashTransaction(
			new TransactionId(Guid.NewGuid()),
			date,
			name,
			type,
			assetId,
			amount,
			currency);

		try
		{
			TransactionTableEntity entity = new CashTransactionTableEntity
			{
				PartitionKey = userId.Value.ToString("N"),
				RowKey = transaction.Id.Value.ToString("N"),
				Date = date.ToString(CultureInfo.InvariantCulture),
				Name = name,
				Type = type.ToString(),
				AssetId = assetId.Value.ToString("N"),
				Amount = amount.ToString(CultureInfo.InvariantCulture),
				Currency = currency.ToString(),
			};

			await this.transactionTableClient.AddEntityAsync(entity, cancellationToken: cancellationToken);

			return transaction;
		}
		catch (Exception ex)
		{
			return Error.Failure(ex.Message);
		}
	}

	private Transaction MapToTransaction(TableEntity entity)
	{
		TransactionType type = Enum.Parse<TransactionType>(entity.GetString("Type"));

		return type switch
		{
			TransactionType.Buy or TransactionType.Sell => new BuySellTransaction(
				new TransactionId(Guid.Parse(entity.RowKey)),
				DateOnly.Parse(entity.GetString("Date"), CultureInfo.InvariantCulture),
				entity.GetString("Name"),
				type,
				new AssetId(Guid.Parse(entity.GetString("AssetId"))),
				decimal.Parse(entity.GetString("Units"), CultureInfo.InvariantCulture)),

			TransactionType.Deposit or TransactionType.Withdrawal or TransactionType.Dividend or TransactionType.Interest or TransactionType.Penalty => new CashTransaction(
				new TransactionId(Guid.Parse(entity.RowKey)),
				DateOnly.Parse(entity.GetString("Date"), CultureInfo.InvariantCulture),
				entity.GetString("Name"),
				type,
				new AssetId(Guid.Parse(entity.GetString("AssetId"))),
				decimal.Parse(entity.GetString("Amount"), CultureInfo.InvariantCulture),
				Enum.Parse<Currency>(entity.GetString("Currency"))),

			_ => throw new NotSupportedException($"Transaction type {type} is not supported"),
		};
	}

	private sealed class BuySellTransactionTableEntity : TransactionTableEntity
	{
		public string Units { get; set; }
	}

	private sealed class CashTransactionTableEntity : TransactionTableEntity
	{
		public string Amount { get; set; }

		public string Currency { get; set; }
	}

	private abstract class TransactionTableEntity : ITableEntity
	{
		public string PartitionKey { get; set; }

		public string RowKey { get; set; }

		public DateTimeOffset? Timestamp { get; set; }

		public ETag ETag { get; set; }

		public string Date { get; set; }

		public string Name { get; set; }

		public string Type { get; set; }

		public string AssetId { get; set; }
	}
}
