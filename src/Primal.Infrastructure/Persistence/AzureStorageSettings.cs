namespace Primal.Infrastructure.Persistence;

internal sealed class AzureStorageSettings
{
	internal const string SectionName = nameof(AzureStorageSettings);

	public string ConnectionString { get; init; }
}
