namespace Primal.Infrastructure.Investments;

internal sealed class InvestmentSettings
{
	internal const string SectionName = nameof(InvestmentSettings);

	public string AlphaVantageApiKey { get; init; }
}
