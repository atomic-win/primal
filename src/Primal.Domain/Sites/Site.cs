using Primal.Domain.Common.Models;

namespace Primal.Domain.Sites;

public sealed class Site : Entity<SiteId>
{
	public Site(SiteId id, Uri url, int dailyLimitInMinutes)
		: base(id)
	{
		this.Url = url;
		this.DailyLimitInMinutes = dailyLimitInMinutes;
	}

	public Uri Url { get; private set; }

	public int DailyLimitInMinutes { get; private set; }
}
