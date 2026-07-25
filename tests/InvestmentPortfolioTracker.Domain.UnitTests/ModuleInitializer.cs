using System.Runtime.CompilerServices;

namespace InvestmentPortfolioTracker.Domain.UnitTests;

internal static class ModuleInitializer
{
	[ModuleInitializer]
	internal static void Initialize()
	{
		VerifierSettings.ScrubInlineGuids();
		VerifierSettings.DontScrubDateTimes();
	}
}
