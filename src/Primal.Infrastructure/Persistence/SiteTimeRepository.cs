using Azure;
using Azure.Data.Tables;
using ErrorOr;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Sites;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Persistence;

internal sealed class SiteTimeRepository : ISiteTimeRepository
{
	private readonly TableClient tableClient;

	internal SiteTimeRepository(TableClient tableClient)
	{
		this.tableClient = tableClient;
	}

	public async Task<ErrorOr<Success>> AddSiteTime(UserId userId, SiteId siteId, DateTime time, CancellationToken cancellationToken)
	{
		time = time.ToUniversalTime().AddTicks(-(time.Ticks % TimeSpan.TicksPerMinute));

		var siteTime = new SiteTimeTableEntity
		{
			PartitionKey = userId.Value.ToString("N"),
			RowKey = time.ToString("O"),
			SiteId = siteId.Value.ToString("N"),
		};

		try
		{
			await this.tableClient.AddEntityAsync(siteTime, cancellationToken: cancellationToken);
			return Result.Success;
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

	private sealed class SiteTimeTableEntity : ITableEntity
	{
		public string PartitionKey { get; set; }

		public string RowKey { get; set; }

		public DateTimeOffset? Timestamp { get; set; }

		public ETag ETag { get; set; }

		public string SiteId { get; set; }
	}
}
