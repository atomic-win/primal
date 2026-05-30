using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Infrastructure.Investments;

namespace Primal.Infrastructure.IntegrationTests.Investments;

public sealed class AssetRepositoryTests
{
	[Test]
	public async Task GetByIdAsync_NonExistent_ReturnsEmptyAsset()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new AssetRepository(db);

		var result = await repository.GetByIdAsync(new AssetId(Guid.NewGuid()), CancellationToken.None);

		await Assert.That(result.Id == AssetId.Empty).IsTrue();
		await Assert.That(string.Equals(result.Name, string.Empty, StringComparison.Ordinal)).IsTrue();
		await Assert.That(result.AssetClass == AssetClass.Unknown).IsTrue();
	}

	[Test]
	public async Task AddAsync_ThenGetByIdAsync_ReturnsCorrectAsset()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new AssetRepository(db);

		var addedAsset = await repository.AddAsync(
			"Global Equity Fund",
			AssetClass.Equity,
			AssetType.MutualFund,
			Currency.USD,
			"mf-12345",
			CancellationToken.None);

		var result = await repository.GetByIdAsync(addedAsset.Id, CancellationToken.None);

		await Assert.That(result.Id == addedAsset.Id).IsTrue();
		await Assert.That(string.Equals(result.Name, "Global Equity Fund", StringComparison.Ordinal)).IsTrue();
		await Assert.That(result.AssetClass == AssetClass.Equity).IsTrue();
		await Assert.That(result.AssetType == AssetType.MutualFund).IsTrue();
		await Assert.That(result.Currency == Currency.USD).IsTrue();
		await Assert.That(string.Equals(result.ExternalId, "mf-12345", StringComparison.Ordinal)).IsTrue();
	}

	[Test]
	public async Task GetByExternalIdAsync_NonExistent_ReturnsEmptyAsset()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new AssetRepository(db);

		var result = await repository.GetByExternalIdAsync("missing-id", CancellationToken.None);

		await Assert.That(result.Id == AssetId.Empty).IsTrue();
		await Assert.That(string.Equals(result.ExternalId, string.Empty, StringComparison.Ordinal)).IsTrue();
	}

	[Test]
	public async Task AddAsync_ThenGetByExternalIdAsync_ReturnsCorrectAsset()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new AssetRepository(db);

		await repository.AddAsync(
			"US Equity ETF",
			AssetClass.Equity,
			AssetType.Stock,
			Currency.USD,
			"stk-456",
			CancellationToken.None);

		var result = await repository.GetByExternalIdAsync("stk-456", CancellationToken.None);

		await Assert.That(result.Id != AssetId.Empty).IsTrue();
		await Assert.That(string.Equals(result.Name, "US Equity ETF", StringComparison.Ordinal)).IsTrue();
		await Assert.That(result.AssetClass == AssetClass.Equity).IsTrue();
		await Assert.That(result.AssetType == AssetType.Stock).IsTrue();
		await Assert.That(result.Currency == Currency.USD).IsTrue();
		await Assert.That(string.Equals(result.ExternalId, "stk-456", StringComparison.Ordinal)).IsTrue();
	}
}
