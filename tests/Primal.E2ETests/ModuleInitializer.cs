using System.Runtime.CompilerServices;
using System.Text;

namespace Primal.E2ETests;

internal static class ModuleInitializer
{
	[ModuleInitializer]
	internal static void Initialize()
	{
		VerifierSettings.ScrubInlineGuids();
		VerifierSettings.DontScrubDateTimes();
		VerifierSettings.AddScrubber(ScrubTraceId);
	}

	private static void ScrubTraceId(StringBuilder input)
	{
		const string prefix = "\"traceId\":\"";
		const string suffix = "\"";

		var text = input.ToString();
		var startIndex = text.IndexOf(prefix, StringComparison.Ordinal);
		if (startIndex < 0)
		{
			return;
		}

		var valueStart = startIndex + prefix.Length;
		var valueEnd = text.IndexOf(suffix, valueStart, StringComparison.Ordinal);
		if (valueEnd < 0)
		{
			return;
		}

		input.Remove(valueStart, valueEnd - valueStart);
		input.Insert(valueStart, "Scrubbed");
	}
}
