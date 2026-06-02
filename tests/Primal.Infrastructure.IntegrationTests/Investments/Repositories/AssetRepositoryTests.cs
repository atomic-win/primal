using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Infrastructure.Investments;

namespace Primal.Infrastructure.IntegrationTests.Investments.Repositories;

public sealed class AssetRepositoryTests
{
	[Test]
	public async Task Add_ThenGetById_ReturnsAsset()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new AssetRepository(db, TimeProvider.System);

		var asset = await repository.AddAsync(
			"Test MF",
			AssetClass.Equity,
			AssetType.MutualFund,
			Currency.INR,
			"mf-119551",
			CancellationToken.None);

		var result = await repository.GetByIdAsync(asset.Id, CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetById_NonExistent_ReturnsEmpty()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new AssetRepository(db, TimeProvider.System);

		var result = await repository.GetByIdAsync(new AssetId(Guid.NewGuid()), CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task Add_ThenGetByExternalId_ReturnsAsset()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new AssetRepository(db, TimeProvider.System);

		var asset = await repository.AddAsync(
			"Test Stock",
			AssetClass.Equity,
			AssetType.Stock,
			Currency.USD,
			"stock-aapl",
			CancellationToken.None);

		var result = await repository.GetByExternalIdAsync("stock-aapl", CancellationToken.None);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task GetByExternalId_NonExistent_ReturnsEmpty()
	{
		var db = TestDbHelper.CreateTestDatabase();
		var repository = new AssetRepository(db, TimeProvider.System);

		var result = await repository.GetByExternalIdAsync("non-existent", CancellationToken.None);

		await Verifier.Verify(result);
	}
}
