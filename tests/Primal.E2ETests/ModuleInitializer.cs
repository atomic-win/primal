using System.Runtime.CompilerServices;

namespace Primal.E2ETests;

internal static class ModuleInitializer
{
	[ModuleInitializer]
	internal static void Initialize()
	{
		VerifierSettings.ScrubInlineGuids();
		VerifierSettings.DontScrubDateTimes();
	}
}
