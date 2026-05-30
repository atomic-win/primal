namespace Primal.UnitTests;

public sealed class SanityTests
{
	[Test]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("TUnit", "TUnitAssertions0005", Justification = "Sanity test")]
	public async Task TestProjectIsConfiguredCorrectly()
	{
		await Assert.That(true).IsTrue();
	}
}
