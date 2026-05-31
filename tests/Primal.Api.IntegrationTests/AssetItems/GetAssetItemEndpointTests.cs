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

public sealed class GetAssetItemEndpointTests
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() },
	};

	[Test]
	public async Task GetAssetItem_Unauthenticated_Returns401()
	{
		await using var factory = new PrimalApiFactory();
		var client = factory.CreateClient();

		var response = await client.GetAsync($"/api/asset-items/{Guid.NewGuid()}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
	}

	[Test]
	public async Task GetAssetItem_NotFound_Returns404()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());

		factory.AssetItemRepository
			.GetByIdAsync(userId, assetItemId, Arg.Any<CancellationToken>())
			.Returns(AssetItem.Empty);

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.GetAsync($"/api/asset-items/{assetItemId.Value}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}

	[Test]
	public async Task GetAssetItem_Exists_ReturnsAssetItemResponse()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var assetId = new AssetId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());
		var assetItem = new AssetItem(assetItemId, assetId, "My Bank");
		var asset = new Asset(assetId, "default-EmergencyFund-BankAccount-INR", AssetClass.EmergencyFund, AssetType.BankAccount, Currency.INR, "default-EmergencyFund-BankAccount-INR");

		factory.AssetItemRepository
			.GetByIdAsync(userId, assetItemId, Arg.Any<CancellationToken>())
			.Returns(assetItem);

		factory.AssetRepository
			.GetByIdAsync(assetId, Arg.Any<CancellationToken>())
			.Returns(asset);

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.GetAsync($"/api/asset-items/{assetItemId.Value}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var body = await response.Content.ReadFromJsonAsync<AssetItemResponse>(JsonOptions);
		await Assert.That(body!.Id).IsEqualTo(assetItemId.Value);
		await Assert.That(body.Name).IsEqualTo("My Bank");
		await Assert.That(body.AssetType).IsEqualTo(AssetType.BankAccount);
		await Assert.That(body.AssetClass).IsEqualTo(AssetClass.EmergencyFund);
		await Assert.That(body.Currency).IsEqualTo(Currency.INR);
	}
}
