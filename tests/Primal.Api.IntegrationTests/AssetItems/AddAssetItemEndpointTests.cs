using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using NSubstitute;
using Primal.Api.AssetItems;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.IntegrationTests.AssetItems;

public sealed class AddAssetItemEndpointTests
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() },
	};

	[Test]
	public async Task AddAssetItem_Unauthenticated_Returns401()
	{
		await using var factory = new PrimalApiFactory();
		var client = factory.CreateClient();

		var request = new AddAssetItemRequest("My Bank", AssetClass.EmergencyFund, AssetType.BankAccount, string.Empty, Currency.INR);
		var response = await client.PostAsJsonAsync("/api/asset-items", request, JsonOptions);

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
	}

	[Test]
	public async Task AddAssetItem_BankAccount_CreatesAssetAndItem()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var assetId = new AssetId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());

		factory.AssetRepository
			.GetByExternalIdAsync("default-EmergencyFund-BankAccount-INR", Arg.Any<CancellationToken>())
			.Returns(Asset.Empty);

		factory.AssetRepository
			.AddAsync("default-EmergencyFund-BankAccount-INR", AssetClass.EmergencyFund, AssetType.BankAccount, Currency.INR, "default-EmergencyFund-BankAccount-INR", Arg.Any<CancellationToken>())
			.Returns(new Asset(assetId, "default-EmergencyFund-BankAccount-INR", AssetClass.EmergencyFund, AssetType.BankAccount, Currency.INR, "default-EmergencyFund-BankAccount-INR"));

		factory.AssetItemRepository
			.AddAsync(userId, assetId, "My Bank", Arg.Any<CancellationToken>())
			.Returns(new AssetItem(assetItemId, assetId, "My Bank"));

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var request = new AddAssetItemRequest("My Bank", AssetClass.EmergencyFund, AssetType.BankAccount, string.Empty, Currency.INR);
		var response = await client.PostAsJsonAsync("/api/asset-items", request, JsonOptions);

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
		await factory.AssetItemRepository.Received(1).AddAsync(userId, assetId, "My Bank", Arg.Any<CancellationToken>());
	}

	[Test]
	public async Task AddAssetItem_MutualFund_NotFound_Returns404()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());

		factory.AssetRepository
			.GetByExternalIdAsync("mf-999999", Arg.Any<CancellationToken>())
			.Returns(Asset.Empty);

		factory.MutualFundApiClient
			.GetBySymbolAsync("999999", Arg.Any<CancellationToken>())
			.Returns(new MutualFund(string.Empty, string.Empty, string.Empty, string.Empty, Currency.Unknown));

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var request = new AddAssetItemRequest("MF", AssetClass.Equity, AssetType.MutualFund, "999999", Currency.Unknown);
		var response = await client.PostAsJsonAsync("/api/asset-items", request, JsonOptions);

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}

	[Test]
	public async Task AddAssetItem_MutualFund_Success_CreatesAssetAndItem()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var assetId = new AssetId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());

		factory.AssetRepository
			.GetByExternalIdAsync("mf-123456", Arg.Any<CancellationToken>())
			.Returns(Asset.Empty);

		factory.MutualFundApiClient
			.GetBySymbolAsync("123456", Arg.Any<CancellationToken>())
			.Returns(new MutualFund("123456", "Test Scheme", "Open Ended", "Equity", Currency.INR));

		factory.AssetRepository
			.AddAsync("Test Scheme", AssetClass.Equity, AssetType.MutualFund, Currency.INR, "mf-123456", Arg.Any<CancellationToken>())
			.Returns(new Asset(assetId, "Test Scheme", AssetClass.Equity, AssetType.MutualFund, Currency.INR, "mf-123456"));

		factory.AssetItemRepository
			.AddAsync(userId, assetId, "My MF", Arg.Any<CancellationToken>())
			.Returns(new AssetItem(assetItemId, assetId, "My MF"));

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var request = new AddAssetItemRequest("My MF", AssetClass.Equity, AssetType.MutualFund, "123456", Currency.Unknown);
		var response = await client.PostAsJsonAsync("/api/asset-items", request, JsonOptions);

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
		await factory.AssetItemRepository.Received(1).AddAsync(userId, assetId, "My MF", Arg.Any<CancellationToken>());
	}
}
