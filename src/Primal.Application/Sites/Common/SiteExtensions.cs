using Primal.Domain.Sites;

namespace Primal.Application.Sites;

internal static class SiteExtensions
{
	private static readonly HashSet<string> AllowedUriSchemes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"http",
		"https",
	};

	internal static bool IsAllowedUriScheme(this Uri uri)
	{
		return AllowedUriSchemes.Contains(uri.Scheme);
	}

	internal static SiteResult ToUnallowedSiteResult(this Uri uri)
	{
		return new SiteResult(new SiteId(Guid.Empty), uri.Host, 0);
	}
}
