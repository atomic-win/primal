using Azure;
using Azure.Data.Tables;
using ErrorOr;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Sites;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Persistence;

internal sealed class SiteRepository : ISiteRepository
{
	private readonly TableClient tableClient;

	internal SiteRepository(TableClient tableClient)
	{
		this.tableClient = tableClient;
	}

	public async Task<ErrorOr<Site>> AddSite(UserId userId, Uri url, int dailyLimitInMinutes, CancellationToken cancellationToken)
	{
		AsyncPageable<SiteTableEntity> entities = this.tableClient.QueryAsync<SiteTableEntity>(
			entity => entity.PartitionKey == userId.Value.ToString("N")
				&& entity.Url == url.Host,
			cancellationToken: cancellationToken);

		await foreach (SiteTableEntity entity in entities.WithCancellation(cancellationToken))
		{
			return Error.Conflict();
		}

		var siteId = SiteId.New();

		var site = new SiteTableEntity
		{
			PartitionKey = userId.Value.ToString("N"),
			RowKey = siteId.Value.ToString("N"),
			Url = url.Host,
			DailyLimitInMinutes = dailyLimitInMinutes,
		};

		try
		{
			await this.tableClient.AddEntityAsync(site, cancellationToken: cancellationToken);
			return new Site(siteId, url.Host, dailyLimitInMinutes);
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

	public async Task<ErrorOr<IEnumerable<Site>>> GetSites(UserId userId, CancellationToken cancellationToken)
	{
		AsyncPageable<SiteTableEntity> entities = this.tableClient.QueryAsync<SiteTableEntity>(
			entity => entity.PartitionKey == userId.Value.ToString("N"),
			cancellationToken: cancellationToken);

		List<Site> sites = new List<Site>();

		await foreach (SiteTableEntity entity in entities.WithCancellation(cancellationToken))
		{
			sites.Add(new Site(new SiteId(Guid.ParseExact(entity.RowKey, "N")), entity.Url, entity.DailyLimitInMinutes));
		}

		return sites;
	}

	public async Task<ErrorOr<Site>> GetSiteById(UserId userId, SiteId siteId, CancellationToken cancellationToken)
	{
		AsyncPageable<SiteTableEntity> entities = this.tableClient.QueryAsync<SiteTableEntity>(
			entity => entity.PartitionKey == userId.Value.ToString("N")
				&& entity.RowKey == siteId.Value.ToString("N"),
			cancellationToken: cancellationToken);

		await foreach (SiteTableEntity entity in entities.WithCancellation(cancellationToken))
		{
			return new Site(new SiteId(Guid.ParseExact(entity.RowKey, "N")), entity.Url, entity.DailyLimitInMinutes);
		}

		return Error.NotFound();
	}

	public async Task<ErrorOr<Site>> GetSiteByUrl(UserId userId, Uri url, CancellationToken cancellationToken)
	{
		AsyncPageable<SiteTableEntity> entities = this.tableClient.QueryAsync<SiteTableEntity>(
			entity => entity.PartitionKey == userId.Value.ToString("N")
				&& entity.Url == url.Host,
			cancellationToken: cancellationToken);

		await foreach (SiteTableEntity entity in entities.WithCancellation(cancellationToken))
		{
			return new Site(new SiteId(Guid.ParseExact(entity.RowKey, "N")), entity.Url, entity.DailyLimitInMinutes);
		}

		return Error.NotFound();
	}

	public async Task<ErrorOr<Success>> UpdateSite(UserId userId, SiteId siteId, int dailyLimitInMinutes, CancellationToken cancellationToken)
	{
		AsyncPageable<SiteTableEntity> entities = this.tableClient.QueryAsync<SiteTableEntity>(
			entity => entity.PartitionKey == userId.Value.ToString("N")
				&& entity.RowKey == siteId.Value.ToString("N"),
			cancellationToken: cancellationToken);

		await foreach (SiteTableEntity entity in entities.WithCancellation(cancellationToken))
		{
			entity.DailyLimitInMinutes = dailyLimitInMinutes;

			try
			{
				await this.tableClient.UpdateEntityAsync(entity, entity.ETag, cancellationToken: cancellationToken);
				return Result.Success;
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

		return Error.NotFound();
	}

	public async Task<ErrorOr<Success>> DeleteSite(UserId userId, SiteId siteId, CancellationToken cancellationToken)
	{
		AsyncPageable<SiteTableEntity> entities = this.tableClient.QueryAsync<SiteTableEntity>(
			entity => entity.PartitionKey == userId.Value.ToString("N")
				&& entity.RowKey == siteId.Value.ToString("N"),
			cancellationToken: cancellationToken);

		await foreach (SiteTableEntity entity in entities.WithCancellation(cancellationToken))
		{
			try
			{
				await this.tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, entity.ETag, cancellationToken: cancellationToken);
				return Result.Success;
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

		return Error.NotFound();
	}

	private sealed class SiteTableEntity : ITableEntity
	{
		public string PartitionKey { get; set; } = default!;

		public string RowKey { get; set; } = default!;

		public DateTimeOffset? Timestamp { get; set; }

		public ETag ETag { get; set; }

		public string Url { get; set; } = default!;

		public int DailyLimitInMinutes { get; set; } = default!;
	}
}
