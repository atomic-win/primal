using System.Runtime.CompilerServices;
using System.Text;

namespace InvestmentPortfolioTracker.E2ETests;

internal static class ModuleInitializer
{
	[ModuleInitializer]
	internal static void Initialize()
	{
		VerifierSettings.ScrubInlineGuids();
		VerifierSettings.DontScrubDateTimes();
		VerifierSettings.AddScrubber(ScrubTraceId);
		VerifierSettings.AddScrubber(ScrubJsonField("accessToken"));
		VerifierSettings.AddScrubber(ScrubJsonField("refreshToken"));
	}

	private static void ScrubTraceId(StringBuilder input)
	{
		ScrubJsonFieldValue(input, "traceId");
	}

	private static Action<StringBuilder> ScrubJsonField(string fieldName)
	{
		return input => ScrubJsonFieldValue(input, fieldName);
	}

	private static void ScrubJsonFieldValue(StringBuilder input, string fieldName)
	{
		var prefix = $"\"{fieldName}\":\"";
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
