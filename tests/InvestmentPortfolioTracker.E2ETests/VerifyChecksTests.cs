namespace InvestmentPortfolioTracker.E2ETests;

public sealed class VerifyChecksTests
{
	[Test]
	public async Task Run() =>
		await VerifyChecks.Run();
}
