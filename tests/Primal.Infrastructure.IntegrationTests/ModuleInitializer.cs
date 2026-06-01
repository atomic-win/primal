using System.Runtime.CompilerServices;

namespace Primal.Infrastructure.IntegrationTests;

internal static class ModuleInitializer
{
	[ModuleInitializer]
	internal static void Initialize()
	{
		VerifierSettings.ScrubInlineGuids();
		VerifierSettings.DontScrubDateTimes();
	}
}
