namespace Primal.Infrastructure.Persistence;

internal static class Constants
{
	internal static class TableNames
	{
		internal const string IdMap = "IdMap";
		internal const string UserIds = "UserIds";
		internal const string Users = "Users";
		internal const string Sites = "Sites";
		internal const string SiteTimes = "SiteTimes";
		internal const string Instruments = "Instruments";
		internal const string MutualFunds = "MutualFunds";
		internal const string Stocks = "Stocks";

		internal static readonly IEnumerable<string> All = new[] { IdMap, UserIds, Users, Sites, SiteTimes, Instruments, MutualFunds, Stocks };
	}
}
