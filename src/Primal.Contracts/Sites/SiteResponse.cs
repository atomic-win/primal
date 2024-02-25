namespace Primal.Contracts.Sites;

public sealed record SiteResponse(Guid Id, Uri Url, int DailyLimitInMinutes);
