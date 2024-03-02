using Primal.Domain.Sites;

namespace Primal.Application.Sites.Common;

public sealed record SiteResult(SiteId Id, string Host, int DailyLimitInMinutes);
