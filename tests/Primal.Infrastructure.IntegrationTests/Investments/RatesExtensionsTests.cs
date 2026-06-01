using Primal.Infrastructure.Investments;

namespace Primal.Infrastructure.IntegrationTests.Investments;

public sealed class RatesExtensionsTests
{
	[Test]
	public async Task GetOnOrBeforeValue_ExactDateExists_ReturnsValue()
	{
		var date = new DateOnly(2024, 6, 15);
		var rates = new Dictionary<DateOnly, decimal>
		{
			{ date, 85.5m },
		};

		var result = rates.GetOnOrBeforeValue(date);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetOnOrBeforeValue_DateDoesNotExist_ReturnsClosestPreviousWithin7Days()
	{
		var date = new DateOnly(2024, 6, 15);
		var rates = new Dictionary<DateOnly, decimal>
		{
			{ date.AddDays(-3), 84.0m },
		};

		var result = rates.GetOnOrBeforeValue(date);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetOnOrBeforeValue_MultipleRatesInLookback_ReturnsClosestToDate()
	{
		var date = new DateOnly(2024, 6, 15);
		var rates = new Dictionary<DateOnly, decimal>
		{
			{ date.AddDays(-1), 84.5m },
			{ date.AddDays(-5), 83.0m },
		};

		var result = rates.GetOnOrBeforeValue(date);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetOnOrBeforeValue_RateAt6DaysBack_ReturnsValue()
	{
		var date = new DateOnly(2024, 6, 15);
		var rates = new Dictionary<DateOnly, decimal>
		{
			{ date.AddDays(-6), 82.0m },
		};

		var result = rates.GetOnOrBeforeValue(date);

		await Verifier.Verify(result);
	}

	[Test]
	public void GetOnOrBeforeValue_NoRateWithin7Days_ThrowsInvalidOperationException()
	{
		var date = new DateOnly(2024, 6, 15);
		var rates = new Dictionary<DateOnly, decimal>
		{
			{ date.AddDays(-7), 81.0m },
		};

		Assert.Throws<InvalidOperationException>(
			() => rates.GetOnOrBeforeValue(date));
	}

	[Test]
	public void GetOnOrBeforeValue_EmptyDictionary_ThrowsInvalidOperationException()
	{
		var date = new DateOnly(2024, 6, 15);
		var rates = new Dictionary<DateOnly, decimal>();

		Assert.Throws<InvalidOperationException>(
			() => rates.GetOnOrBeforeValue(date));
	}
}
