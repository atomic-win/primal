using Primal.Domain.Sites;

namespace Primal.Application.Sites;

public sealed record SiteResult(SiteId Id, string Url, int DailyLimitInMinutes);
