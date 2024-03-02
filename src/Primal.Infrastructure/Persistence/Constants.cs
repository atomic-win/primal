namespace Primal.Infrastructure.Persistence;

internal static class Constants
{
	internal static class TableNames
	{
		internal const string Users = "Users";
		internal const string Sites = "Sites";
		internal const string UserIds = "UserIds";

		internal static readonly IEnumerable<string> All = new[] { Users, Sites, UserIds };
	}
}
