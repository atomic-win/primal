namespace Primal.Infrastructure.Persistence;

internal sealed class PersistenceSettings
{
	internal const string SectionName = "Persistence";

	public LiteDBSettings LiteDB { get; init; }
}
