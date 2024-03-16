namespace Primal.Infrastructure.Persistence;

internal static class Constants
{
	internal static class TableNames
	{
		internal const string UserIds = "UserIds";
		internal const string Users = "Users";
		internal const string Sites = "Sites";
		internal const string SiteTimes = "SiteTimes";

		internal static readonly IEnumerable<string> All = new[] { UserIds, Users, Sites, SiteTimes };
	}
}
