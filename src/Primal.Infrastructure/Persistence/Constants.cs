namespace Primal.Infrastructure.Persistence;

internal static class Constants
{
	internal static class TableNames
	{
		internal const string UserIds = "UserIds";
		internal const string Users = "Users";
		internal const string Sites = "Sites";
		internal const string SiteTimes = "SiteTimes";
		internal const string InstrumentIdMapping = "InstrumentIdMapping";
		internal const string Instruments = "Instruments";
		internal const string Assets = "Assets";
		internal const string Transactions = "Transactions";

		internal static readonly IEnumerable<string> All =
		[
			UserIds,
			Users,
			Sites,
			SiteTimes,
			InstrumentIdMapping,
			Instruments,
			Assets,
			Transactions,
		];
	}
}
