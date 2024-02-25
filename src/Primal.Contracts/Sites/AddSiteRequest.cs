namespace Primal.Contracts.Sites;

public sealed record AddSiteRequest(Uri Url, int DailyLimitInMinutes);
