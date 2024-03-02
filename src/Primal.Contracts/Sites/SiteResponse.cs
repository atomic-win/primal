namespace Primal.Contracts.Sites;

public sealed record SiteResponse(Guid Id, string Host, int DailyLimitInMinutes);
