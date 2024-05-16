using Azure;
using Azure.Data.Tables;
using ErrorOr;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Infrastructure.Persistence;

internal sealed class StockRepository : IStockRepository
{
	private readonly TableClient idMapTableClient;
	private readonly TableClient stockTableClient;

	internal StockRepository(TableClient idMapTableClient, TableClient stockTableClient)
	{
		this.idMapTableClient = idMapTableClient;
		this.stockTableClient = stockTableClient;
	}

	public async Task<ErrorOr<Stock>> GetBySymbolAsync(string symbol, CancellationToken cancellationToken)
	{
		try
		{
			StockSymbolTableEntity entity = await this.idMapTableClient.GetEntityAsync<StockSymbolTableEntity>(
				"StockSymbol",
				symbol,
				cancellationToken: cancellationToken);

			return await this.GetByIdAsync(new StockId(Guid.Parse(entity.StockId)), cancellationToken);
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

	public async Task<ErrorOr<Stock>> GetByIdAsync(StockId stockId, CancellationToken cancellationToken)
	{
		try
		{
			StockTableEntity entity = await this.stockTableClient.GetEntityAsync<StockTableEntity>(
				stockId.Value.ToString("N"),
				"Stock",
				cancellationToken: cancellationToken);

			return new Stock(
				stockId,
				entity.Symbol,
				entity.Name,
				entity.Region,
				entity.Currency);
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

	public async Task<ErrorOr<Stock>> AddAsync(string symbol, string name, string region, Currency currency, CancellationToken cancellationToken)
	{
		try
		{
			StockId stockId = StockId.New();

			StockSymbolTableEntity idMapEntity = new StockSymbolTableEntity
			{
				RowKey = symbol,
				StockId = stockId.Value.ToString("N"),
			};

			StockTableEntity entity = new StockTableEntity
			{
				PartitionKey = stockId.Value.ToString("N"),
				Symbol = symbol,
				Name = name,
				Region = region,
				Currency = currency,
			};

			await this.stockTableClient.AddEntityAsync(entity, cancellationToken);
			await this.idMapTableClient.AddEntityAsync(idMapEntity, cancellationToken);

			return new Stock(stockId, symbol, name, region, currency);
		}
		catch (RequestFailedException ex) when (ex.Status == 409)
		{
			return Error.Conflict();
		}
		catch (Exception ex)
		{
			return Error.Failure(ex.Message);
		}
	}

	private sealed class StockSymbolTableEntity : ITableEntity
	{
		public string PartitionKey { get; set; } = "StockSymbol";

		public string RowKey { get; set; } = string.Empty;

		public DateTimeOffset? Timestamp { get; set; }

		public ETag ETag { get; set; }

		public string StockId { get; set; } = string.Empty;
	}

	private sealed class StockTableEntity : ITableEntity
	{
		public string PartitionKey { get; set; } = string.Empty;

		public string RowKey { get; set; } = "Stock";

		public DateTimeOffset? Timestamp { get; set; }

		public ETag ETag { get; set; }

		public string Symbol { get; set; } = string.Empty;

		public string Name { get; set; } = string.Empty;

		public string Region { get; set; } = string.Empty;

		public Currency Currency { get; set; }
	}
}
