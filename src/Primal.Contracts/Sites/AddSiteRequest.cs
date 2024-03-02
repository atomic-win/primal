namespace Primal.Contracts.Sites;

public sealed record AddSiteRequest(Uri Host, int DailyLimitInMinutes);
