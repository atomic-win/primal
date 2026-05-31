using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using NSubstitute;
using Primal.Api.AssetItems;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.IntegrationTests.AssetItems;

public sealed class GetAllAssetItemsEndpointTests
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() },
	};

	[Test]
	public async Task GetAllAssetItems_Unauthenticated_Returns401()
	{
		await using var factory = new PrimalApiFactory();
		var client = factory.CreateClient();

		var response = await client.GetAsync("/api/asset-items");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
	}

	[Test]
	public async Task GetAllAssetItems_Empty_ReturnsEmptyArray()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());

		factory.AssetItemRepository
			.GetAllAsync(userId, Arg.Any<CancellationToken>())
			.Returns(Array.Empty<AssetItem>());

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.GetAsync("/api/asset-items");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadFromJsonAsync<List<AssetItemResponse>>(JsonOptions) ?? [];
		await Assert.That(body.Count).IsEqualTo(0);
	}

	[Test]
	public async Task GetAllAssetItems_WithItems_ReturnsAssetItems()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var firstAssetId = new AssetId(Guid.NewGuid());
		var secondAssetId = new AssetId(Guid.NewGuid());
		var firstAssetItemId = new AssetItemId(Guid.NewGuid());
		var secondAssetItemId = new AssetItemId(Guid.NewGuid());
		var assetItems = new[]
		{
			new AssetItem(firstAssetItemId, firstAssetId, "My MF"),
			new AssetItem(secondAssetItemId, secondAssetId, "Cash Wallet"),
		};
		var firstAsset = new Asset(firstAssetId, "Fund", AssetClass.Equity, AssetType.MutualFund, Currency.INR, "mf-123");
		var secondAsset = new Asset(secondAssetId, "Wallet", AssetClass.EmergencyFund, AssetType.Wallet, Currency.USD, "wallet-1");

		factory.AssetItemRepository
			.GetAllAsync(userId, Arg.Any<CancellationToken>())
			.Returns(assetItems);

		factory.AssetRepository
			.GetByIdAsync(firstAssetId, Arg.Any<CancellationToken>())
			.Returns(firstAsset);

		factory.AssetRepository
			.GetByIdAsync(secondAssetId, Arg.Any<CancellationToken>())
			.Returns(secondAsset);

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.GetAsync("/api/asset-items");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadFromJsonAsync<List<AssetItemResponse>>(JsonOptions) ?? [];
		await Assert.That(body.Count).IsEqualTo(2);
		await Assert.That(body[0].Id).IsEqualTo(firstAssetItemId.Value);
		await Assert.That(body[0].Name).IsEqualTo("My MF");
		await Assert.That(body[0].AssetType).IsEqualTo(AssetType.MutualFund);
		await Assert.That(body[0].AssetClass).IsEqualTo(AssetClass.Equity);
		await Assert.That(body[0].Currency).IsEqualTo(Currency.INR);
		await Assert.That(body[1].Id).IsEqualTo(secondAssetItemId.Value);
		await Assert.That(body[1].Name).IsEqualTo("Cash Wallet");
		await Assert.That(body[1].AssetType).IsEqualTo(AssetType.Wallet);
		await Assert.That(body[1].AssetClass).IsEqualTo(AssetClass.EmergencyFund);
		await Assert.That(body[1].Currency).IsEqualTo(Currency.USD);
	}
}
