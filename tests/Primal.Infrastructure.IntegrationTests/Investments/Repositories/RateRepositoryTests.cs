using System.Collections.Frozen;
using Microsoft.Extensions.Time.Testing;
using Primal.Infrastructure.Investments;

namespace Primal.Infrastructure.IntegrationTests.Investments.Repositories;

public sealed class RateRepositoryTests
{
	private static readonly DateTimeOffset FrozenTime = new(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

	[Test]
	public async Task GetRecentRates_NoData_ReturnsEmpty()
	{
		// Arrange
		var (repository, _) = CreateRepository();

		// Act
		var result = await repository.GetRecentRatesAsync("AAPL", RateType.Stock, CancellationToken.None);

		// Assert
		await Verifier.Verify(result);
	}

	[Test]
	public async Task AddRates_ThenGetRecentRates_ReturnsRates()
	{
		// Arrange
		var (repository, _) = CreateRepository();
		var rates = new Dictionary<DateOnly, decimal>
		{
			[new DateOnly(2026, 5, 28)] = 150.25m,
			[new DateOnly(2026, 5, 29)] = 151.50m,
			[new DateOnly(2026, 5, 30)] = 149.75m,
		}.ToFrozenDictionary();

		// Act
		await repository.AddRatesAsync("AAPL", RateType.Stock, rates, CancellationToken.None);
		var result = await repository.GetRecentRatesAsync("AAPL", RateType.Stock, CancellationToken.None);

		// Assert
		await Verifier.Verify(result);
	}

	[Test]
	public async Task AddRates_EmptyDictionary_DoesNotThrow()
	{
		// Arrange
		var (repository, _) = CreateRepository();
		var rates = new Dictionary<DateOnly, decimal>().ToFrozenDictionary();

		// Act
		await repository.AddRatesAsync("AAPL", RateType.Stock, rates, CancellationToken.None);
		var result = await repository.GetRecentRatesAsync("AAPL", RateType.Stock, CancellationToken.None);

		// Assert
		await Verifier.Verify(result);
	}

	[Test]
	public async Task AddRates_DuplicateCall_OnlyWritesMissingDates()
	{
		// Arrange
		var (repository, _) = CreateRepository();
		var firstBatch = new Dictionary<DateOnly, decimal>
		{
			[new DateOnly(2026, 5, 28)] = 150.25m,
			[new DateOnly(2026, 5, 29)] = 151.50m,
		}.ToFrozenDictionary();

		var secondBatch = new Dictionary<DateOnly, decimal>
		{
			[new DateOnly(2026, 5, 28)] = 999.99m,
			[new DateOnly(2026, 5, 29)] = 999.99m,
			[new DateOnly(2026, 5, 30)] = 149.75m,
		}.ToFrozenDictionary();

		// Act
		await repository.AddRatesAsync("AAPL", RateType.Stock, firstBatch, CancellationToken.None);
		await repository.AddRatesAsync("AAPL", RateType.Stock, secondBatch, CancellationToken.None);
		var result = await repository.GetRecentRatesAsync("AAPL", RateType.Stock, CancellationToken.None);

		// Assert
		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetRecentRates_StaleData_ReturnsEmpty()
	{
		// Arrange
		var (repository, timeProvider) = CreateRepository();
		var rates = new Dictionary<DateOnly, decimal>
		{
			[new DateOnly(2026, 5, 20)] = 150.25m,
		}.ToFrozenDictionary();

		await repository.AddRatesAsync("AAPL", RateType.Stock, rates, CancellationToken.None);

		timeProvider.Advance(TimeSpan.FromDays(8));

		// Act
		var result = await repository.GetRecentRatesAsync("AAPL", RateType.Stock, CancellationToken.None);

		// Assert
		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetRecentRates_DifferentRateTypes_AreIsolated()
	{
		// Arrange
		var (repository, _) = CreateRepository();
		var stockRates = new Dictionary<DateOnly, decimal>
		{
			[new DateOnly(2026, 5, 28)] = 150.25m,
		}.ToFrozenDictionary();

		var mutualFundRates = new Dictionary<DateOnly, decimal>
		{
			[new DateOnly(2026, 5, 28)] = 45.67m,
		}.ToFrozenDictionary();

		await repository.AddRatesAsync("AAPL", RateType.Stock, stockRates, CancellationToken.None);
		await repository.AddRatesAsync("AAPL", RateType.MutualFund, mutualFundRates, CancellationToken.None);

		// Act
		var stockResult = await repository.GetRecentRatesAsync("AAPL", RateType.Stock, CancellationToken.None);
		var mutualFundResult = await repository.GetRecentRatesAsync("AAPL", RateType.MutualFund, CancellationToken.None);

		// Assert
		await Verifier.Verify(new { stockResult, mutualFundResult });
	}

	[Test]
	public async Task GetRecentRates_DifferentSymbols_AreIsolated()
	{
		// Arrange
		var (repository, _) = CreateRepository();
		var aaplRates = new Dictionary<DateOnly, decimal>
		{
			[new DateOnly(2026, 5, 28)] = 150.25m,
		}.ToFrozenDictionary();

		var googRates = new Dictionary<DateOnly, decimal>
		{
			[new DateOnly(2026, 5, 28)] = 2800.00m,
		}.ToFrozenDictionary();

		await repository.AddRatesAsync("AAPL", RateType.Stock, aaplRates, CancellationToken.None);
		await repository.AddRatesAsync("GOOG", RateType.Stock, googRates, CancellationToken.None);

		// Act
		var aaplResult = await repository.GetRecentRatesAsync("AAPL", RateType.Stock, CancellationToken.None);
		var googResult = await repository.GetRecentRatesAsync("GOOG", RateType.Stock, CancellationToken.None);

		// Assert
		await Verifier.Verify(new { aaplResult, googResult });
	}

	[Test]
	public async Task AddRates_NormalizesSymbolToUpperCase()
	{
		// Arrange
		var (repository, _) = CreateRepository();
		var rates = new Dictionary<DateOnly, decimal>
		{
			[new DateOnly(2026, 5, 28)] = 150.25m,
		}.ToFrozenDictionary();

		// Act
		await repository.AddRatesAsync("aapl", RateType.Stock, rates, CancellationToken.None);
		var result = await repository.GetRecentRatesAsync("AAPL", RateType.Stock, CancellationToken.None);

		// Assert
		await Verifier.Verify(result);
	}

	private static (RateRepository Repository, FakeTimeProvider TimeProvider) CreateRepository()
	{
		var timeProvider = new FakeTimeProvider(FrozenTime);
		var repository = new RateRepository(TestDbFactory.CreateTestDatabase(), timeProvider);
		return (repository, timeProvider);
	}
}
