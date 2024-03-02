using Primal.Domain.Sites;

namespace Primal.Application.Sites;

public sealed record AddSiteResult(SiteId Id, string Host, int DailyLimitInMinutes);
