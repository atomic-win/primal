using Primal.Domain.Common.Models;

namespace Primal.Domain.Sites;

public sealed class Site : Entity<SiteId>
{
	public Site(SiteId id, string host, int dailyLimitInMinutes)
		: base(id)
	{
		this.Host = host;
		this.DailyLimitInMinutes = dailyLimitInMinutes;
	}

	public string Host { get; private set; }

	public int DailyLimitInMinutes { get; private set; }
}
